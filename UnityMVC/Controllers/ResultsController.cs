using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnityMVC.Models;
using System.IO;

namespace UnityMVC.Controllers
{
    public class ResultsController : Controller
    {
        public ActionResult Index()
        {
            return View(new GameResultsContext().GameResults.ToArray());
        }

        public ActionResult PushResult(string password, string leftTag, string rightTag, int leftScore, int rightScore, string logFileName)
        {
            
            if (password != WebConstants.WebPassword)
                return new ContentResult { Content = "Password fail" };
            var users = new UsersContext().UserProfiles.ToArray();
            var tags = users.Select(u => u.CvarcTag).ToArray();
            if (!tags.Contains(leftTag) || !tags.Contains(rightTag))
                return new ContentResult {Content = "tag fail"};
            var context = new GameResultsContext();
            context.GameResults.Add(new GameResults
            {
                Time = DateTime.Now,
                LeftPlayerUserName = users.First(u => u.CvarcTag == leftTag).UserName,
                RightPlayerUserName = users.First(u => u.CvarcTag == rightTag).UserName,
                LeftPlayerScores = leftScore,
                RightPlayerScores = rightScore,
                logFileName = logFileName
            });
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }
        [HttpPost]
        public ActionResult PushLog(string password, HttpPostedFileBase file)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult { Content = "fail" };

            if (file == null)
                return new ContentResult {Content = "sorry"};
            var fname = System.IO.Path.GetFileName(file.FileName);
            file.SaveAs(WebConstants.BasePath + WebConstants.RelativeLogPath + fname);
            return new ContentResult {Content = "suc"};
        }

        public ActionResult SayStatus(string password, bool isOnline)
        {
            if (password != WebConstants.WebPassword)
                return new ContentResult {Content = "password fail"};
            var context = new UnityStatusContext();
            var status = context.UnityStatus.First();
            if (status.Online == isOnline)
                return new ContentResult {Content = "already know!"};
            status.Online = isOnline;
            if (isOnline)
                status.UpTime = DateTime.Now;
            context.SaveChanges();
            return new ContentResult {Content = "suc"};
        }

        [Authorize]
        [HttpGet]
        public ActionResult UploadSolution()
        {
            var baseFileName = WebConstants.BasePath + WebConstants.RelativeSolutionsPath + User.Identity.Name;
            var solutionExists = System.IO.File.Exists(baseFileName + ".zip") ||
                                 System.IO.File.Exists(baseFileName + ".rar");
            if (solutionExists)
            {
                var time = new UsersContext().UserProfiles.First(u => u.UserName == User.Identity.Name).SolutionLoaded;
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
            if (extention != "zip" && extention != "rar")
            {
                ViewBag.Message = "Ошибка: вы должны предоставить архив с решением в формате *.rar или *.zip";
                return View(simpleFileView);
            }
            var expectedFileName = User.Identity.Name + "." + extention;
            simpleFileView.UploadedFile.SaveAs(path + expectedFileName);
            var context = new UsersContext();
            context.UserProfiles.First(u => u.UserName == User.Identity.Name).SolutionLoaded = DateTime.Now;
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