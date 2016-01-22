using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace UnityMVP.Models
{
    public class GameResults
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string LeftPlayerUserName { get; set; }
        public string RightPlayerUserName { get; set; }
        public int LeftPlayerScores { get; set; }
        public int RightPlayerScores { get; set; }
        public string logFileName { get; set; }
    }

    public class GameResultsContext : DbContext
    {
        public GameResultsContext() : base("GameResultsContext")
        {
        }

        public DbSet<GameResults> GameResults { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}