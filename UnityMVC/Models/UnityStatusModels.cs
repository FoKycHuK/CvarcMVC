using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace UnityMVC.Models
{
    public class UnityStatus
    {
        public int Id { get; set; }
        public bool Online { get; set; }
        public DateTime? UpTime { get; set; }
    }
}