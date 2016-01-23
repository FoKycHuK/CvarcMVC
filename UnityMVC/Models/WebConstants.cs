using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UnityMVC.Models
{
    public static class WebConstants
    {
        public static string SuperAdminLogin = "admin";
        public static string SuperAdminPassword = "somePassword";
        public static string WebPassword = "somePassword";
        public static string RelativeLogPath = "Content/UnityLogs/";
        public static string AbsoluteLogPath = AppDomain.CurrentDomain.BaseDirectory + RelativeLogPath;
    }
}