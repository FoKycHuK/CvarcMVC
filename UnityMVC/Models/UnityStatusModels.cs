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

    public class UnityStatusContext : DbContext
    {
        public UnityStatusContext() : base("UnityStatusContext")
        {
        }

        public DbSet<UnityStatus> UnityStatus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}