using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using WebMatrix.WebData;

namespace UnityMVP.Models
{
    public class MyDatabaseInit : DropCreateDatabaseAlways<UsersContext>
    {
        //
        // GET: /MyDatabaseInit/
        protected override void Seed(UsersContext context)
        {
            WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            InitRolesAndDefaultAccount();
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
        }

       


}
}
