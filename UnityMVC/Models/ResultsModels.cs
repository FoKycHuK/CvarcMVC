using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace UnityMVC.Models
{
    public class GameResults
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string LeftPlayerUserName { get; set; }
        public string RightPlayerUserName { get; set; }
        public int LeftPlayerScores { get; set; }
        public int RightPlayerScores { get; set; }
        public string LogFileName { get; set; }
        public string Type { get; set; } // enum страшно, int непонятно.
        public string Subtype { get; set; }
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

    public class UnityStatistics
    {
        public bool IsOnline { get; set; }
        public int GamePlayed { get; set; }
        public int UniquePlayers { get; set; }
        public DateTime? UpTime { get; set; }
    }

    public class GroupResults
    {
        IReadOnlyList<GameResults> results;
        public GroupResults(IReadOnlyList<GameResults> groupResults)
        {
            results = groupResults;
            CheckForErrors(); //??
        }

        public IEnumerable<string> GetAllGroupNames()
        {
            return results
                .Select(r => r.Subtype)
                .Distinct();
        }

        public IEnumerable<string> GetAllPlayersForGroup(string group)
        {
            return results
                .Where(r => r.Subtype == group)
                .SelectMany(r => new[] { r.LeftPlayerUserName, r.RightPlayerUserName })
                .Distinct()
                .OrderByDescending(p => GetTotalScoreForPlayer(p, group));
        }

        public GameResults GetResultForPair(string player1, string player2, string group)
        {
            return results.FirstOrDefault(r =>
                r.Subtype == group &&
                (r.LeftPlayerUserName == player1 &&
                r.RightPlayerUserName == player2) ||
                (r.LeftPlayerUserName == player2 &&
                r.RightPlayerUserName == player1));
        }

        public string GetLineForPair(string player1, string player2, string group)
        {
            if (player1 == player2)
                return null;
            var game = GetResultForPair(player1, player2, group);
            if (game == null)
                return "Не сыграно";
            var shortUserName1 = player1.Length > 3
                ? player1.Substring(0, 3) : player1;
            var shortUserName2 = player2.Length > 3
                ? player2.Substring(0, 3) : player2;
            var player1score = game.LeftPlayerUserName == player1 ? game.LeftPlayerScores : game.RightPlayerScores;
            var player2score = game.LeftPlayerUserName == player2 ? game.LeftPlayerScores : game.RightPlayerScores;

            return player1score + " : " + player2score;
        }

        public string GetLogPathForPair(string player1, string player2, string group)
        {
            var game = GetResultForPair(player1, player2, group);
            return game == null ? "404" : game.LogFileName;
        }

        public int GetTotalScoreForPlayer(string player, string group)
        {
            return results.Where(r =>
                    r.Subtype == group &&
                    (r.LeftPlayerUserName == player ||
                    r.RightPlayerUserName == player))
                .Sum(r => 
                    r.LeftPlayerUserName == player ? 
                    r.LeftPlayerScores : 
                    r.RightPlayerScores);
        }

        private void CheckForErrors()
        {
            foreach (var group in GetAllGroupNames())
                foreach (var player1 in GetAllPlayersForGroup(group))
                    foreach (var player2 in GetAllPlayersForGroup(group))
                    {
                        var gamesCount = results.Count(r =>
                            r.Subtype == group &&
                            (r.LeftPlayerUserName == player1 &&
                            r.RightPlayerUserName == player2) ||
                            (r.LeftPlayerUserName == player2 &&
                            r.RightPlayerUserName == player1));
                        if (player1 == player2 && gamesCount > 0)
                            throw new Exception("Игрок " + player1 + " играл сам с собой!");
                        if (gamesCount > 1)
                            throw new Exception("Игроки " + player1 + " и " + player2 + " играли между собой более одного раза!");
                    }
        }
    }
}