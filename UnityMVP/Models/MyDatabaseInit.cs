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

            WebSecurity.CreateUserAndAccount("qweqwe", "qweqwe");
            WebSecurity.CreateUserAndAccount("qweqweqwe", "qweqwe");
            var userContext = new UsersContext();
            userContext.UserProfiles.FirstOrDefault(u => u.UserName == "qweqwe").CvarcTag = "12f3f648-97dc-4743-a605-a8ced438ed5d";
            userContext.UserProfiles.FirstOrDefault(u => u.UserName == "qweqweqwe").CvarcTag = "6808f2b8-e626-4d78-9ccb-fd2670689f96";
            userContext.SaveChanges();
        }


        public class MyDatabaseInitCometitions : DropCreateDatabaseAlways<CompetitionsContext>
        {
            protected override void Seed(CompetitionsContext context)
            {
                context.Competitions.Add(new Competition()
                {
                    CreatedBy = "admin",
                    Name = "Default competition",
                    Description = "This is a sample of a competition rules. Hope its seems good :3",
                    UnityName = "RoboMoviesLevel1",
                    PlayedGames = new List<GameResult> {new GameResult {Id = 1, LeftPlayer = "left", RightPlayer = "right", LeftScore = 10, RightScore = 20} },
                    StartAt = DateTime.Now,
                    IsActive = true
                });
                context.SaveChanges();
            }
        }

}
}
