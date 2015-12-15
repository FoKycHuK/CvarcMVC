using System;
using System.Collections.Generic;
using System.Linq;
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
            var competition = context.Competitions.ToArray();
            return View(competition);
            //return new ContentResult()
            //{
            //    Content =
            //        string.Format("Name: {0}, Description: {1}. Is Active: {2}", competition.Name,
            //            competition.Description, competition.IsActiive)
            //};
        }

        public ActionResult CompetitionInfo(string name)
        {
            return new ContentResult() {Content = name};
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Add()
        {
            return new ContentResult() {Content = "not implemented"};
        }
    }
}