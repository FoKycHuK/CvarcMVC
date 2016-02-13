using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnityMVC.Models;

namespace UnityMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult UnityStatus()
        {
            var context = new UnityContext();
            var hourAgo = DateTime.Now - TimeSpan.FromHours(1);
            var lastHoursGames = context.GameResults.Where(r => r.Time > hourAgo);
            var gamesCount = lastHoursGames.Count();
            var uniquePlayersCount = 
                lastHoursGames.Select(r => r.LeftPlayerUserName)
                .Union(lastHoursGames.Select(r => r.RightPlayerUserName))
                .Count();
            var status = context.UnityStatus.First();
            var model = new UnityStatistics
            {
                IsOnline = status.Online,
                GamePlayed = gamesCount,
                UniquePlayers = uniquePlayersCount,
                UpTime = status.Online ? status.UpTime : null
            };
            return PartialView("_UnityStatusPartial", model);
        }
    }
}
