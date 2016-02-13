//using System;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using System.Threading;
//using System.Web.Mvc;
//using System.Web.Security;
//using WebMatrix.WebData;
//using UnityMVC.Models;
//using System.Linq;

//namespace UnityMVC.Filters
//{
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
//    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
//    {
//        private static SimpleMembershipInitializer _initializer;
//        private static object _initializerLock = new object();
//        private static bool _isInitialized;

//        public override void OnActionExecuting(ActionExecutingContext filterContext)
//        {
//            // Ensure ASP.NET Simple Membership is initialized only once per app start
//            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
//        }

//        private class SimpleMembershipInitializer
//        {
//            public SimpleMembershipInitializer()
//            {
//                Database.SetInitializer<UsersContext>(null);

//                try
//                {
//                    using (var context = new UsersContext())
//                    {
//                        if (context.Database.Exists())
//                            context.Database.Delete(); // delete this line when release to cancel always drop&create.

//                        if (!context.Database.Exists())
//                        {
//                            // Create the SimpleMembership database without Entity Framework migration schema
//                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
//                        }
//                    }

//                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
//                    InitRolesAndDefaultAccount();
//                }
//                catch (Exception ex)
//                {
//                    throw new InvalidOperationException("Ошибка при инициализации базы данных с пользователями.", ex);
//                }
//            }
            
//        }
//    }
//}
