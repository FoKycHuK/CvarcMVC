using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnityMVP.Models;

namespace UnityMVP.Controllers
{
    public class RulesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Results()
        {
            return View(new GameResultsContext().GameResults.ToArray());
        }

        public ActionResult PushResult(string password, string leftTag, string rightTag, int leftScore, int rightScore)
        {
            
            if (password != WebConstants.WebPassword)
                return new ContentResult { Content = "Password fail" };
            var users = new UsersContext().UserProfiles.ToArray();
            var tags = users.Select(u => u.CvarcTag).ToArray();
            if (!tags.Contains(leftTag) || !tags.Contains(rightTag))
                return new ContentResult {Content = "tag fail"};
            var context = new GameResultsContext();
            context.GameResults.Add(new GameResults
            {
                Time = DateTime.Now,
                LeftPlayerUserName = users.First(u => u.CvarcTag == leftTag).UserName,
                RightPlayerUserName = users.First(u => u.CvarcTag == rightTag).UserName,
                LeftPlayerScores = leftScore,
                RightPlayerScores = rightScore
            });
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }
    }
}