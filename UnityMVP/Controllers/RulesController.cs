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

        public ActionResult PushResult(string password, string leftTag, string rightTag, int leftScore, int rightScore, string logFileName)
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
                RightPlayerScores = rightScore,
                logFileName = logFileName
            });
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }
        [HttpPost]
        public ActionResult PushLog(string password, HttpPostedFileBase file)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult { Content = "fail" };

            if (file == null)
                return new ContentResult {Content = "sorry"};
            var fname = System.IO.Path.GetFileName(file.FileName);
            file.SaveAs(WebConstants.AbsoluteLogPath + fname);
            return new ContentResult {Content = "suc"};
        }

        public ActionResult SayStatus(string password, bool isOnline)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult {Content = "password fail"};
            var context = new UnityStatusContext();
            var status = context.UnityStatus.First();
            if (status.Online == isOnline)
                return new ContentResult {Content = "already know!"};
            status.Online = isOnline;
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }
    }
}