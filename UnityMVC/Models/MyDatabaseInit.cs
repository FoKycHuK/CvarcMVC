using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Security;
using UnityMVC.Models;
using WebMatrix.WebData;

namespace UnityMVC.Models
{
    public class InitUnityDb : CreateDatabaseIfNotExists<UnityContext>
    {
        protected override void Seed(UnityContext context)
        {
            //if (!context.Database.Exists())
            //    ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();

            WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            InitRolesAndDefaultAccount(context);

            context.GameResults.Add(new GameResults
            {
                Time = WebConstants.GetCurrentTime(),
                LeftPlayerUserName = "test0",
                RightPlayerUserName = "test1",
                LeftPlayerScores = 10,
                RightPlayerScores = 20,
                LogFileName = "hehkektop.txt",
                Type = "training"
            });

            MakeSomeGroupGamesForTests(context);


            context.UnityStatus.Add(new UnityStatus { Online = false });
            context.SaveChanges();
        }

        private void MakeSomeGroupGamesForTests(UnityContext context)
        {
            if (System.IO.File.Exists(WebConstants.BasePath + "Content/testGames.txt"))
            {
                var lines = System.IO.File.ReadAllLines(WebConstants.BasePath + "Content/testGames.txt");
                var splited =
                    lines.Where(line => !line.StartsWith("//") && !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Split(':'));
                // example: lName:rName:10:20:some.log:A
                var games = splited.Select(s => new GameResults
                {
                    Time = WebConstants.GetCurrentTime(),
                    LeftPlayerUserName = s[0],
                    RightPlayerUserName = s[1],
                    LeftPlayerScores = int.Parse(s[2]),
                    RightPlayerScores = int.Parse(s[3]),
                    LogFileName = s[4],
                    Type = s[5],
                    Subtype = s[6]
                });
                foreach (var game in games)
                    context.GameResults.Add(game);
            }
        }

        private void InitRolesAndDefaultAccount(UnityContext context)
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

            WebSecurity.CreateUserAndAccount("test0", "s0mePassword");
            WebSecurity.CreateUserAndAccount("test1", "s0mePassword");
            WebSecurity.CreateUserAndAccount("test2", "s0mePassword");
            WebSecurity.CreateUserAndAccount("test3", "s0mePassword");
            WebSecurity.CreateUserAndAccount("Buggy_bot", "s0mePassword");
            WebSecurity.CreateUserAndAccount("Aggro_bot", "s0mePassword");
            WebSecurity.CreateUserAndAccount("Standing_bot", "s0mePassword");
            WebSecurity.CreateUserAndAccount("Correct_bot", "s0mePassword");
            context.UserProfiles.First(u => u.UserName == "test0").CvarcTag = "00000000-0000-0000-0000-000000000000";
            context.UserProfiles.First(u => u.UserName == "test1").CvarcTag = "00000000-0000-0000-0000-000000000001";
            context.UserProfiles.First(u => u.UserName == "test2").CvarcTag = "00000000-0000-0000-0000-000000000002";
            context.UserProfiles.First(u => u.UserName == "test3").CvarcTag = "00000000-0000-0000-0000-000000000003";
            context.UserProfiles.First(u => u.UserName == "Buggy_bot").CvarcTag = "00000000-0000-0000-0000-000000000005";
            context.UserProfiles.First(u => u.UserName == "Aggro_bot").CvarcTag = "00000000-0000-0000-0000-000000000006";
            context.UserProfiles.First(u => u.UserName == "Standing_bot").CvarcTag = "00000000-0000-0000-0000-000000000007";
            context.UserProfiles.First(u => u.UserName == "Correct_bot").CvarcTag = "00000000-0000-0000-0000-000000000008";
        }
    }
}
