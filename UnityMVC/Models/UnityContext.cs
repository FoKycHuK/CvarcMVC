using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using WebMatrix.WebData;

namespace UnityMVC.Models
{
    public class UnityContext : DbContext
    {
        public UnityContext(bool needToInitialize = true)
            : base("DefaultConnection")
        {
            if (!WebSecurity.Initialized && needToInitialize)
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GameResults> GameResults { get; set; }
        public DbSet<UnityStatus> UnityStatus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}