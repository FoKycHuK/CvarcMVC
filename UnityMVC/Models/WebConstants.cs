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
        public static string RelativeSolutionsPath = "Content/Solutions/";
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string Gmail = "cvarc.team@gmail.com";
        public static string GmailPassword = "somePassword";
        public static int CountOfPlayoffPlayers = 8;
    }
}