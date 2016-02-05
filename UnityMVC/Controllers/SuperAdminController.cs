using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UnityMVC.Models;
using WebMatrix.WebData;

namespace UnityMVC.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class SuperAdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterUser(RegisterModel model)
        {
            if (WebSecurity.UserExists(model.UserName))
            {
                ViewBag.Message =
                    "Такой пользователь уже есть!";
                return View(model);
            }
            if (!RegisterModel.IsCorrectUserName(model.UserName))
            {
                ViewBag.Message =
                    "В имени пользователя допустимы только рус/англ буквы, точка, пробел, земля и дефис.";
                return View(model);
            }
            WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
            var context = new UsersContext();
            context.UserProfiles.First(z => z.UserName == model.UserName).CvarcTag = Guid.NewGuid().ToString();
            context.SaveChanges();
            ViewBag.Message = "Аккаунт создан успешно";
            return View();
        }

        [HttpGet]
        public ActionResult RegisterManyUsers()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterManyUsers(string users)
        {
            var splited = users.Split('\n').Select(line => line.Split(';')).ToArray();
            var uncorrect = splited.Where(s => 
            s.Length < 2 ||
            !RegisterModel.IsCorrectUserName(s[0]) || 
            s[1].Length < 6 || 
            WebSecurity.UserExists(s[0]))
            .ToArray();
            if (uncorrect.Any())
            {
                ViewBag.Message = "Что-то не так(пароль меньше 6 или не указан, в имени некорректные символы, пользователь уже сущ-вует) для пользователей: ";
                foreach (var value in uncorrect)
                    ViewBag.Message += string.Join(";", value);
                return View();
            }
            foreach (var value in splited)
                WebSecurity.CreateUserAndAccount(value[0], value[1]);
            var userNames = splited.Select(s => s[0]);
            var context = new UsersContext();
            foreach (var value in splited)
            {
                var user = context.UserProfiles.First(u => u.UserName == value[0]);
                user.CvarcTag = Guid.NewGuid().ToString();
                if (value.Length > 2)
                    user.Email = value[2];
                if (value.Length > 3)
                    user.SocialLink = value[3];
            }
            context.SaveChanges();
            ViewBag.Message = "Пользователи успешно зарегистрированы";
            return View();
        }

        [HttpGet]
        public ActionResult DeleteUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeleteUser(FormCollection collection)
        {
            var userToDelete = collection["UserName"];
            try
            {
                if (Roles.GetRolesForUser(userToDelete).Any())
                    Roles.RemoveUserFromRoles(userToDelete, Roles.GetRolesForUser(userToDelete));
                ((SimpleMembershipProvider) Membership.Provider).DeleteAccount(userToDelete);
                    // deletes record from webpages_Membership table
                ((SimpleMembershipProvider) Membership.Provider).DeleteUser(userToDelete, true);
                    // deletes record from UserProfile table
                ViewBag.ResultMessage = "Пользователь удален.";
            }
            catch (NullReferenceException)
            {
                ViewBag.ResultMessage = "Пользователь не существует.";
            }
            catch
            {
                ViewBag.ResultMessage = "Неизвестная ошибка при удалении пользователя.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult GrantAdminAccess()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult GrantAdminAccess(FormCollection collection)
        {
            var userToGrant = collection["UserName"];
            try
            {
                var superAdmin = bool.Parse(collection["SuperAdminAccess"].Split(',')[0]);
                var roleToGrant = superAdmin ? "SuperAdmin" : "Admin";
                if (WebSecurity.UserExists(userToGrant))
                {
                    if (!Roles.IsUserInRole(userToGrant, roleToGrant))
                    {
                        Roles.AddUserToRole(userToGrant, roleToGrant);
                        ViewBag.ReturnMessage = "Права администратора успешно присвоены пользователю " + userToGrant;
                    }
                    else
                        ViewBag.ReturnMessage = "Этот пользователь уже имеет права администратора такого уровня.";
                }
                else
                    ViewBag.ReturnMessage = "Пользователь не существует";
            }
            catch
            {
                ViewBag.ReturnMessage = "Неизвсестная ошибка при назначении прав.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult CleanUp()
        {
            return View();
        }

        public ActionResult CleanUp(FormCollection collection)
        {
            if (collection["UserName"] != User.Identity.Name)
            {
                ViewBag.Message = "Неверно введено ваше имя пользователя";
                return View();
            }
            var context = new GameResultsContext();
            foreach (var value in context.GameResults)
                context.GameResults.Remove(value);
            context.SaveChanges();
            ViewBag.Message = "Все результаты игр удалены.";
            return View();
        }
    }
}
