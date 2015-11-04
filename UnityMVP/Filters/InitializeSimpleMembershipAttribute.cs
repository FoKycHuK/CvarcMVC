using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using UnityMVP.Models;

namespace UnityMVP.Filters
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
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }

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
            }
        }
    }
}
