using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnityMVP.Models;

namespace UnityMVP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            //return Redirect("/SuperAdmin/");
            return View();
        }

        [Authorize]
        public string Test()
        {
            var name = User.Identity.Name;
            var tag = new UsersContext().UserProfiles.Where(z => z.UserName == name).First().CvarcTag;//Select(z => z.CvarcTag).FirstOrDefault();
            return name + " " + tag;
        }
    }
}
