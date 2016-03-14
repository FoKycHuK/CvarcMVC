using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnityMVC.Models;
using System.IO;
using CVARC.Infrastructure;

namespace UnityMVC.Controllers
{
    public class ResultsController : Controller
    {
        public ActionResult Index()
        {
            return View(new UnityContext().GameResults.Where(r => r.Type == "training" && r.RightPlayerUserName != null).OrderByDescending(r => r.Time).ToArray());
        }

        public ActionResult GroupResults()
        {
            var collection = new UnityContext().GameResults.Where(r => r.Type == "group");
            return View(new GroupResults(collection.ToList()));
        }

        public ActionResult TournamentResult()
        {
            var collection = new UnityContext().GameResults.Where(r => r.Type == "tournament");
            return View(new TournamentResults(collection.ToList(), WebConstants.CountOfPlayoffPlayers));
        }

        public ActionResult SoloResult()
        {
            var collection = new UnityContext().GameResults.Where(r => r.RightPlayerUserName == null).OrderByDescending(r => r.Time).ToArray();
            return View(collection);
        }

        [HttpPost]
        public ActionResult PushResult(GameResultInfo result)
        {
            if (result.PushPassword != WebConstants.WebPassword)
                return new ContentResult { Content = "Password fail" };
            var context = new UnityContext();
            var users = context.UserProfiles.ToArray();
            var tags = users.Select(u => u.CvarcTag).ToArray();
            if (result.Players.Length == 2)
            {
                if (!tags.Contains(result.Players[0].CvarcTag) || !tags.Contains(result.Players[1].CvarcTag))
                    return new ContentResult {Content = "tag fail"};
                
                context.GameResults.Add(new GameResults
                {
                    Time = WebConstants.GetCurrentTime(),
                    LeftPlayerUserName = users.First(u => u.CvarcTag == result.Players[0].CvarcTag).UserName,
                    RightPlayerUserName = users.First(u => u.CvarcTag == result.Players[1].CvarcTag).UserName,
                    LeftPlayerScores = result.Players[0].Score,
                    RightPlayerScores = result.Players[1].Score,
                    LogFileName = result.LogFileName,
                    Type = result.Tag,
                    Subtype = result.Subtag
                });
                context.SaveChanges();
                return new ContentResult {Content = "successful"};
            }

            var player = result.Players[0];

            if (!tags.Contains(player.CvarcTag))
                return new ContentResult { Content = "tag fail" };

            context.GameResults.Add(new GameResults
            {
                Time = WebConstants.GetCurrentTime(),
                LeftPlayerUserName = users.First(u => u.CvarcTag == player.CvarcTag).UserName,
                LeftPlayerScores = player.Score,
                LogFileName = result.LogFileName,
                Type = result.Tag,
                Subtype = result.Subtag
            });
            context.SaveChanges();
            return new ContentResult { Content = "successful" };

        }
        [HttpPost]
        public ActionResult PushLog(string password, HttpPostedFileBase file)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult { Content = "fail" };

            if (file == null)
                return new ContentResult {Content = "sorry"};
            var fname = Path.GetFileName(file.FileName);
            file.SaveAs(WebConstants.BasePath + WebConstants.RelativeLogPath + fname);
            return new ContentResult {Content = "successful"};
        }

        public ActionResult SayStatus(string password, bool isOnline)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult {Content = "password fail"};
            var context = new UnityContext();
            var status = context.UnityStatus.First();
            if (status.Online == isOnline)
                return new ContentResult {Content = "already know!"};
            status.Online = isOnline;
            if (isOnline)
                status.UpTime = WebConstants.GetCurrentTime();
            context.SaveChanges();
            return new ContentResult {Content = "successful"};
        }

        [Authorize]
        [HttpGet]
        public ActionResult UploadSolution()
        {
            var baseFileName = WebConstants.BasePath + WebConstants.RelativeSolutionsPath + User.Identity.Name;
            var solutionExists = System.IO.File.Exists(baseFileName + ".zip");
            if (solutionExists)
            {
                var time = new UnityContext().UserProfiles.First(u => u.UserName == User.Identity.Name).SolutionLoaded;
                string valueToDisplay = time == null ? "неизвестно" : time.ToString();
                ViewBag.Message = "Ваше решение уже загружено. Время загрузки " +
                                  valueToDisplay +
                                  ". Вы можете его перезаписать. В этом случае, старое решение будет недоступно.";
            }
            return View(new SimpleFileView());
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadSolution(SimpleFileView simpleFileView)
        {
            if (!Directory.Exists(WebConstants.BasePath + WebConstants.RelativeSolutionsPath))
                Directory.CreateDirectory(WebConstants.BasePath + WebConstants.RelativeSolutionsPath);
            if (simpleFileView.UploadedFile == null)
            {
                ViewBag.Message = "Ошибка: Выберете файл для загрузки!";
                return View(simpleFileView);
            }
            if (simpleFileView.UploadedFile.ContentLength > 7 * 512 * 1024) // 3.5 MB
            {
                ViewBag.Message = "Ошибка: Файл слишком большой! Максимальный размер 3.5 МБ. Попробуйте удалить все бинарные файлы из архива.";
                return View(simpleFileView);
            }
            var path = WebConstants.BasePath + WebConstants.RelativeSolutionsPath;
            var extention = simpleFileView.UploadedFile.FileName.Split('.').Last();
            if (extention != "zip")
            {
                ViewBag.Message = "Ошибка: вы должны предоставить архив с решением в формате *.zip";
                return View(simpleFileView);
            }
            var expectedFileName = User.Identity.Name + "." + extention;
            simpleFileView.UploadedFile.SaveAs(path + expectedFileName);
            var context = new UnityContext();
            context.UserProfiles.First(u => u.UserName == User.Identity.Name).SolutionLoaded = WebConstants.GetCurrentTime();
            context.SaveChanges();
            ViewBag.Message = "Решение было загружено успешно!";
            return View(simpleFileView);
        }

        [Authorize]
        public ActionResult DownloadSolution()
        {
            var baseFilePath = WebConstants.BasePath + WebConstants.RelativeSolutionsPath + User.Identity.Name;
            var solutionExists = System.IO.File.Exists(baseFilePath + ".zip") ||
                                 System.IO.File.Exists(baseFilePath + ".rar");
            if (!solutionExists)
                return new ContentResult {Content = "Ваше решение не найдено!"};
            var extension = (System.IO.File.Exists(baseFilePath + ".zip") ? ".zip" : ".rar");
            return new FilePathResult(baseFilePath + extension, "multipart/form-data") {FileDownloadName = User.Identity.Name + extension};
        }
    }
}