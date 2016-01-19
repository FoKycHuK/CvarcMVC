using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace UnityMVP.Models
{
    public class Competition
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        [DisplayName("Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")] //HH:mm:ss
        [DataType(DataType.DateTime)]
        public DateTime StartAt { get; set; }
        [DisplayName("Is active now")]
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string UnityName { get; set; }
        public ICollection<GameResult> PlayedGames { get; set; }
    }

    public class GameResult
    {
        public int Id { get; set; }
        public string LeftPlayer { get; set; }
        public string RightPlayer { get; set; }
        public int LeftScore { get; set; }
        public int RightScore { get; set; }
    }

    public class CompetitionsContext : DbContext
    {
        public CompetitionsContext() : base("CompetitionsContext") { }

        public DbSet<Competition> Competitions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}