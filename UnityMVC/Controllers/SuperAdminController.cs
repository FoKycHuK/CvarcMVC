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
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {


        //
        // GET: /SuperAdmin/

        public ActionResult Index()
        {
            return View();
        }

        // GET: /Roles/Create
        [HttpGet]
        public ActionResult DeleteUser()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
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
    }
}
