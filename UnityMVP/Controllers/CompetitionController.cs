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
            comp.Comments = new List<Comment>();
            var context = new CompetitionsContext();
            context.Competitions.Add(comp);
            context.SaveChanges();
            return new ContentResult { Content = "Competition " + comp.Name + " created." };
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(string name)
        {
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
    }
}