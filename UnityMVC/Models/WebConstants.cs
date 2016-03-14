using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UnityMVC.Models
{
    public static class WebConstants
    {
        public static string SuperAdminLogin = "admin";
        public static string SuperAdminPassword = "s0mePassword";
        public static string WebPassword = "s0mePassword";
        public static string RelativeLogPath = "Content/UnityLogs/";
        public static string RelativeSolutionsPath = "Content/Solutions/";
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string Gmail = "cvarc.team@gmail.com";
        public static string GmailPassword = "somePassword";
        public static int CountOfPlayoffPlayers = 16;
        public static bool IsRegistrationAvailable = false; // not so constant, but...
        public static bool IsRecreateTagAvailable = true;

        public static DateTime GetCurrentTime()
        {
            return DateTime.UtcNow + TimeSpan.FromHours(5);
        }
    }
}