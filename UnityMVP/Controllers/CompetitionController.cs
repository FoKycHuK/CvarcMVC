using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using UnityMVP.Models;

namespace UnityMVP.Controllers
{
    public class CompetitionController : Controller
    {
        public ActionResult Index()
        {
            var context = new CompetitionsContext();
            var competitions = context.Competitions.ToArray();
            return View(competitions);
        }

        public ActionResult CompetitionInfo(string name)
        {
            var context = new CompetitionsContext();
            var competition = context.Competitions.Include(c => c.PlayedGames).FirstOrDefault(c => c.Name == name);
            if (competition == null)
                return new ContentResult { Content = "Competition " + name + " not found!" };
            return View(competition);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ActionResult Add(Competition comp)
        {
            comp.CreatedBy = User.Identity.Name;
            comp.PlayedGames = new List<GameResult>();
            var context = new CompetitionsContext();
            context.Competitions.Add(comp);
            context.SaveChanges();
            return new ContentResult { Content = "Competition " + comp.Name + " created." };
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(string name)
        {
            var context = new CompetitionsContext();
            var competition = context.Competitions.FirstOrDefault(c => c.Name == name);
            competition.IsActive = !competition.IsActive;

            return new ContentResult {Content = "editing not implemented. delete competition and create another one :("};
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(string name)
        {
            var context = new CompetitionsContext();
            var compToDelete = context.Competitions.FirstOrDefault(c => c.Name == name);
            if (compToDelete == null)
                return new ContentResult {Content = "competition not found"};
            context.Competitions.Remove(compToDelete);
            context.SaveChanges();
            return new ContentResult {Content = "competition deleted"};
        }

        [AllowAnonymous]
        public ActionResult SendResult(string password, string leftTag, string rightTag, 
            int leftScore, int rightScore, string competitionName)
        {
            var context = new CompetitionsContext();
            var usersContext = new UsersContext();
            var tags = usersContext.UserProfiles.Select(p => p.CvarcTag).ToList();
            var competition = context.Competitions.Include(c => c.PlayedGames).FirstOrDefault(c => c.UnityName == competitionName);
            if (!tags.Contains(leftTag))
                return new ContentResult {Content = "left tag fail"};
            if (!tags.Contains(rightTag))
                return new ContentResult { Content = "right tag fail: " + rightTag };
            if (competition == null)
                return new ContentResult { Content = "competition fail" };
            if (password != "somePassword")
                return new ContentResult {Content = "Password fail"};
            if (!competition.IsActive)
                return new ContentResult {Content = "competition is not active"};
            var leftPlayerName = usersContext.UserProfiles.First(u => u.CvarcTag == leftTag).UserName;
            var rightPlayerName = usersContext.UserProfiles.First(u => u.CvarcTag == rightTag).UserName;
            competition.PlayedGames.Add(new GameResult {LeftPlayer = leftPlayerName, LeftScore = leftScore, RightPlayer = rightPlayerName, RightScore = rightScore});
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }
    }
}