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
                logFileName = "hehkektop.txt"
            });
            context.SaveChanges();
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
