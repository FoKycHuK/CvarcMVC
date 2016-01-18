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
            var competitions = context.Competitions.ToArray();
            return View(competitions);
        }

        public ActionResult CompetitionInfo(string name)
        {
            var context = new CompetitionsContext();
            var competition = context.Competitions.FirstOrDefault(c => c.Name == name);
            if (competition == null)
                return new ContentResult { Content = "Competition " + name + " not found!" };
            return View(competition);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Add()
        {
            return new ContentResult() {Content = "not implemented"};
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(string name)
        {
            throw new NotImplementedException();
        }
    }
}