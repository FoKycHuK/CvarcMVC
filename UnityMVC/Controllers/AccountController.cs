using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using UnityMVC.Filters;
using UnityMVC.Models;

namespace UnityMVC.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Имя пользователя или пароль неверны.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var allowedSymbols = "_ -.";
            var allowedLetters = "qwertyuiopasdfghjklzxcvbnmйцукенгшщзхъфывапролджэячсмитьбю"; // я не использую char.IsLetter чтобы избежать арабских и подобных символов.
            var allowedChars = allowedSymbols + allowedLetters + allowedLetters.ToUpper();
            // Attempt to register the user
            try
            {
                var username = model.UserName;
                if (username.Any(ch => !allowedChars.Contains(ch)))
                {
                    ViewBag.Message =
                        "В имени пользователя допустимы только рус/англ буквы, точка, пробел, земля и дефис.";
                    return View(model);
                }
                WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new {Email = model.Email});
                var context = new UsersContext();
                context.UserProfiles.First(z => z.UserName == model.UserName).CvarcTag = Guid.NewGuid().ToString();
                context.SaveChanges();
                WebSecurity.Login(model.UserName, model.Password);
                return RedirectToAction("Index", "Home");
            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            var user = new UsersContext().UserProfiles.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Пользователя с таким email не существует.";
                return View();
            }
            var token = WebSecurity.GeneratePasswordResetToken(user.UserName);
            
            string body = "Hello! \nYour username: " + user.UserName + "\nYour token: " + token;
            ViewBag.Message = TrySendEmail(email, "Password recovery", body)
                ? "Письмо отправлено."
                : "Произошла ошибка отправки письма. Обратитесь к администратору :(";
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword, string confirm)
        {
            if (newPassword != confirm)
            {
                ViewBag.Message = "Ошибка: пароли не совпадают";
                return View();
            }
            ViewBag.Message = WebSecurity.ResetPassword(token, newPassword) ? "Пароль успешно изменен" : "Ошибка: плохой токен";
            return View();
        }

        public ActionResult RecreateCvarcTag()
        {
            var context = new UsersContext();
            context.UserProfiles.First(u => u.UserName == User.Identity.Name).CvarcTag = Guid.NewGuid().ToString();
            context.SaveChanges();
            return RedirectToAction("Manage", new {Message = ManageMessageId.RecreateCvarcTagSuccess});
        }

        private bool TrySendEmail(string email, string subj, string body)
        {
            var client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(WebConstants.Gmail, WebConstants.GmailPassword);

            var msg = new MailMessage();
            msg.From = new MailAddress(WebConstants.Gmail);
            msg.To.Add(new MailAddress(email));
            msg.Subject = subj;
            msg.Body = body;
            try
            {
                client.Send(msg);
            }
            catch
            {
                return false;
            }
            return true;
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            var context = new UsersContext();
            var user = context.UserProfiles.FirstOrDefault(z => z.UserName == User.Identity.Name);

            ViewBag.CvarcTag = (user == null || string.IsNullOrEmpty(user.CvarcTag))
                ? "У вас нет кварк тега и вы не можете играть. Возможно, что-то пошло не так."
                : user.CvarcTag;
            ViewBag.PlayedGames = new GameResultsContext().GameResults
                .Where(r => r.LeftPlayerUserName == User.Identity.Name || r.RightPlayerUserName == User.Identity.Name)
                .ToArray();
            ViewBag.SolutionLoadedTime = user.SolutionLoaded == null ? "неизвестно" : user.SolutionLoaded.ToString();
            var baseFileName = WebConstants.BasePath + WebConstants.RelativeSolutionsPath + User.Identity.Name;
            ViewBag.SolutionExists = System.IO.File.Exists(baseFileName + ".zip") ||
                                     System.IO.File.Exists(baseFileName + ".rar");
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Ваш пароль установлен."
                : message == ManageMessageId.RemoveLoginSuccess ? "Дополнительный способ входа удален."
                : message == ManageMessageId.RecreateCvarcTagSuccess ? "Вам присвоен новый CvarcTag. Старый больше не активен."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Текущий пароль неверный, либо новый пароль некорректный.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "Такой пользователь уже существует. Выберете другое имя.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RecreateCvarcTagSuccess
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Такой пользователь уже существует. Выберете другое имя.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Пользователь с таким e-mail уже существует. Выберете другой e-mail.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Пароль неверный. Проверьте и попробуйте еще раз";

                case MembershipCreateStatus.InvalidEmail:
                    return "e-mail неверный. Проверьте и попробуйте еще раз";

                case MembershipCreateStatus.InvalidAnswer:
                    return "Неправильный ответ на вопрос. Проверьте и попробуйте еще раз";

                case MembershipCreateStatus.InvalidQuestion:
                    return "Неправильный вопрос. Проверьте и попробуйте еще раз";

                case MembershipCreateStatus.InvalidUserName:
                    return "Неправильное имя пользователя. Проверьте и попробуйте еще раз";

                case MembershipCreateStatus.ProviderError:
                    return "Ошибка аутентификации. Проверьте данные и попробуйте еще раз. Если ошибка повторяется -- свяжитесь с администратором";

                case MembershipCreateStatus.UserRejected:
                    return "Отменено. Попробуйте еще раз. Если ошибка повторяется -- свяжитесь с администратором";

                default:
                    return "Неизвестная ошибка. Попробуйте еще раз. Если ошибка повторяется -- свяжитесь с администратором";
            }
        }
        #endregion
    }
}
