using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SourceBrowser.Generator;
using SourceBrowser.Site.Models;
using SourceBrowser.Site.Repositories;

namespace SourceBrowser.Site.Controllers
{
    public class BrowseController : Controller
    {
        // GET: Browse
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LookupFile(string id)
        {
            if (BrowserRepository.IsFile(id))
            {
                string githubUser;
                string githubRepo;
                string solutionName;
                string fileName;
                BrowserRepository.GetFolderInfo(id, out githubUser, out githubRepo, out solutionName, out fileName);

                if (!String.IsNullOrEmpty(fileName))
                {
                    var docInfo = BrowserRepository.FindFile(id);
                    var viewModel = BrowserRepository.SetUpFileStructure(docInfo, githubUser, githubRepo, solutionName, fileName);
                    return View("LookupFile", viewModel);
                }
            }
            return View("LookupError");
        }

        public ActionResult LookupFolder(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                var users = BrowserRepository.GetAllGithubUsers();
                ViewBag.Users = users;
                return View("Index");
            }

            string githubUser;
            string githubRepo;
            string solutionName;
            string fileName;
            BrowserRepository.GetFolderInfo(id, out githubUser, out githubRepo, out solutionName, out fileName);

            try
            {
                if (!String.IsNullOrEmpty(solutionName))
                {
                    var viewModel = BrowserRepository.SetUpSolutionStructure(githubUser, githubRepo, solutionName);
                    return View("LookupFolder", viewModel);
                }
                else if (!String.IsNullOrEmpty(githubRepo))
                {
                    var viewModel = BrowserRepository.SetUpRepoStructure(githubUser, githubRepo);
                    return View("LookupRepo", viewModel);
                }
                else if (!String.IsNullOrEmpty(githubUser))
                {
                    var viewModel = BrowserRepository.SetUpUserStructure(githubUser);
                    return View("LookupUser", viewModel);
                }
                else
                {
                    return View("LookupError");
                }
            }
            catch
            {
                return View("LookupError");
            }
        }
    }
}