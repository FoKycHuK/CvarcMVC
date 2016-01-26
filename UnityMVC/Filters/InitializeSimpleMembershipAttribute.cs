using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using UnityMVC.Models;
using System.Linq;

namespace UnityMVC.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<UsersContext>(null);

                try
                {
                    using (var context = new UsersContext())
                    {
                        context.Database.Delete(); // delete this line when release to cancel always drop&create.

                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
                    InitRolesAndDefaultAccount();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Ошибка при инициализации базы данных с пользователями.", ex);
                }
            }
            private void InitRolesAndDefaultAccount()
            {
                // Включаем роли
                if (!Roles.Enabled)
                    Roles.Enabled = true;

                // Создаем две роли
                if (!Roles.RoleExists("SuperAdmin"))
                    Roles.CreateRole("SuperAdmin");
                if (!Roles.RoleExists("Admin"))
                    Roles.CreateRole("Admin");

                // Создаем администратора и дает ему супер права.
                if (!WebSecurity.UserExists(WebConstants.SuperAdminLogin))
                    WebSecurity.CreateUserAndAccount(WebConstants.SuperAdminLogin, WebConstants.SuperAdminPassword);
                if (!Roles.IsUserInRole(WebConstants.SuperAdminLogin, "SuperAdmin"))
                    Roles.AddUserToRole(WebConstants.SuperAdminLogin, "SuperAdmin");

                // for tests
                if (!WebSecurity.UserExists("smalladmin"))
                    WebSecurity.CreateUserAndAccount("smalladmin", WebConstants.SuperAdminPassword);
                if (!Roles.IsUserInRole("smalladmin", "Admin"))
                    Roles.AddUserToRole("smalladmin", "Admin");

                WebSecurity.CreateUserAndAccount("test0", "qweqwe");
                WebSecurity.CreateUserAndAccount("test1", "qweqwe");
                WebSecurity.CreateUserAndAccount("test2", "qweqwe");
                WebSecurity.CreateUserAndAccount("test3", "qweqwe");
                WebSecurity.CreateUserAndAccount("test4", "qweqwe");
                WebSecurity.CreateUserAndAccount("test5", "qweqwe");
                WebSecurity.CreateUserAndAccount("test6", "qweqwe");
                var userContext = new UsersContext();
                userContext.UserProfiles.First(u => u.UserName == "test0").CvarcTag = "00000000-0000-0000-0000-000000000000";
                userContext.UserProfiles.First(u => u.UserName == "test1").CvarcTag = "00000000-0000-0000-0000-000000000001";
                userContext.SaveChanges();
            }
        }
    }
}
