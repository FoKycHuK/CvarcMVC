using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using UnityMVC.Models;
using WebMatrix.WebData;

namespace UnityMVC.Models
{
    public class InitGameResultsDB : DropCreateDatabaseAlways<GameResultsContext>
    {
        protected override void Seed(GameResultsContext context)
        {
            context.GameResults.Add(new GameResults
            {
                Time = DateTime.Now,
                LeftPlayerUserName = "test0",
                RightPlayerUserName = "test1",
                LeftPlayerScores = 10,
                RightPlayerScores = 20,
                LogFileName = "hehkektop.txt",
                Type = "training"
            });
            
            MakeSomeGroupGamesForTests(context);

            context.SaveChanges();
        }

        private void MakeSomeGroupGamesForTests(GameResultsContext context)
        {
            var lines = System.IO.File.ReadAllLines(WebConstants.BasePath + "Content/testGames.txt");
            var splited = lines.Select(line => line.Split(':'));
            // example: lName:rName:10:20:some.log:A
            var games = splited.Select(s => new GameResults
            {
                Time = DateTime.Now,
                LeftPlayerUserName = s[0],
                RightPlayerUserName = s[1],
                LeftPlayerScores = int.Parse(s[2]),
                RightPlayerScores = int.Parse(s[3]),
                LogFileName = s[4],
                Type = "group",
                Subtype = s[5]
            });
            foreach (var game in games)
                context.GameResults.Add(game);
        }
    }

    public class UnityStatusDB : DropCreateDatabaseAlways<UnityStatusContext>
    {
        protected override void Seed(UnityStatusContext context)
        {
            context.UnityStatus.Add(new UnityStatus {Online = false});
            context.SaveChanges();
        }
    }
}
