using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UnityMVC.Models;
using System.Data.SqlClient;
using System.Threading;

namespace UnityMVC
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            Database.SetInitializer<UnityContext>(new InitUnityDb());

            //initialize now
            //это позволяет отлавливать ошибки с БД сразу же, а не потом.
            var context = new UnityContext(false);

            // опасный трай. если вдруг на сервере сработает -- вся база умрет.
            // наверное.
            // но без этого локальная не работает.
            // файл с бд отсутствует, а база данных почему-то есть.
            // ниже строчка с дропом должна быть всегда закомментирована в гите на всякий случай.
            // при необходимости запустить локально -- у себя локально раскомментировать.
            // это лучшее решение, которое я придумал
            try
            {
                context.UnityStatus.ToArray();
            }
            catch (DataException)
            {
                //context.Database.Delete();
                context = new UnityContext(false);
                context.UnityStatus.ToArray();
            }
            
        }
    }
}