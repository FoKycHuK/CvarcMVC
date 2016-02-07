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
                ((r.LeftPlayerUserName == player1 &&
                r.RightPlayerUserName == player2) ||
                (r.LeftPlayerUserName == player2 &&
                r.RightPlayerUserName == player1)));
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
                            ((r.LeftPlayerUserName == player1 &&
                            r.RightPlayerUserName == player2) ||
                            (r.LeftPlayerUserName == player2 &&
                            r.RightPlayerUserName == player1)));
                        if (player1 == player2 && gamesCount > 0)
                            throw new Exception("Игрок " + player1 + " играл сам с собой!");
                        if (gamesCount > 1)
                            throw new Exception("Игроки " + player1 + " и " + player2 + " играли между собой более одного раза! ");
                    }
        }
    }

    public class TournamentResults
    {
        IReadOnlyList<GameResults> results;
        public Element[][] lines;
        public bool ready;
        private int totalPlayers;
        private GameResults[] lowBracketGames; //first round
        public TournamentResults(IReadOnlyList<GameResults> tournamentResults, int totalPlayers)
        {
            results = tournamentResults;
            this.totalPlayers = totalPlayers;

            var lowestRoundSubtag = "1/" + (totalPlayers/2);
            lowBracketGames = results
                .Where(r => r.Subtype == lowestRoundSubtag)
                .OrderBy(r => r.Time)
                .ToArray();
            if (lowBracketGames.Length != totalPlayers/2)
                return;
            ready = true;

            lines = Enumerable.Range(0, totalPlayers).Select(GetLineByIndex).ToArray();
        }

        private Element[] GetLineByIndex(int index)
        {
            var line = new List<Element>();

            for (var i = 1; i <= totalPlayers; i *= 2)
            {
                bool needScores = i != 1; //true
                bool needToAdd = NeedToAdd(index, i);
                if (needScores && needToAdd)
                    line.Add(GetElementByIndex(index, i, true));
                if (needToAdd)
                    line.Add(GetElementByIndex(index, i, false));
            }
            return line.ToArray();
        }

        private bool NeedToAdd(int index, int span)
        {
            return index % span == 0;
        }

        private string GetLink(string username)
        {
            var user = new UsersContext().UserProfiles.FirstOrDefault(u => u.UserName == username);
            return user == null ? null : user.SocialLink;
        }

        private Element GetElementByIndex(int index, int span, bool isScores)
        {
            if (span > 1)
            {
                var gameNum = index/span;
                var game = GetGame(span/2, gameNum);
                if (isScores)
                {
                    var text = game == null ? "Не сыграно" : game.LeftPlayerScores + " : " + game.RightPlayerScores;
                    return new Element(text, game == null ? null : game.LogFileName, span, false);
                }
                var winner = game == null ? "" : 
                    game.LeftPlayerScores > game.RightPlayerScores
                    ? game.LeftPlayerUserName
                    : game.RightPlayerUserName;
                return new Element(winner, GetLink(winner), span, true);
            }
            var num = index/2;
            var lowBracketGame = lowBracketGames[num];
            var firstRoundPlayer = index%2 == 0 ? lowBracketGame.LeftPlayerUserName : lowBracketGame.RightPlayerUserName;
            return new Element(firstRoundPlayer, GetLink(firstRoundPlayer), span, true);
        }

        private GameResults GetGame(int span, int number)
        {
            if (span <= 1)
                return lowBracketGames[number];
            var firstGame = GetGame(span / 2,  number * 2);
            var secondGame = GetGame(span / 2, number * 2 + 1);
            if (firstGame == null || secondGame == null)
                return null;
            var winner1 = firstGame.LeftPlayerScores > firstGame.RightPlayerScores
                ? firstGame.LeftPlayerUserName 
                : firstGame.RightPlayerUserName;
            var winner2 = secondGame.LeftPlayerScores > secondGame.RightPlayerScores
                ? secondGame.LeftPlayerUserName
                : secondGame.RightPlayerUserName;
            return GetGameByPlayers(winner1, winner2);
        }

        private GameResults GetGameByPlayers(string player1, string player2)
        {
            return results.FirstOrDefault(r => 
            (r.LeftPlayerUserName == player1 && r.RightPlayerUserName == player2) ||
            (r.LeftPlayerUserName == player2 && r.RightPlayerUserName == player1));
        }

        public class Element
        {
            public Element(string text, string link, int span, bool isPlayerName)
            {
                this.text = text;
                this.link = link;
                this.span = span;
                this.isPlayerName = isPlayerName;
            }

            public readonly int span;
            public readonly string text;
            public readonly string link;
            public readonly bool isPlayerName;
        }
    }
}