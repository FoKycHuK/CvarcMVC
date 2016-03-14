using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UnityMVC.Models
{
    public class Forbidden : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 403;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}