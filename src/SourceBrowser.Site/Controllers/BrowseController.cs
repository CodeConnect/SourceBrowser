namespace SourceBrowser.Site.Controllers
{
    using System;
    using System.Web.Mvc;

    using SourceBrowser.Site.Repositories;
    using System.IO;
    using SourceBrowser.Site.Attributes;
    using SourceBrowser.Shared;

    public class BrowseController : Controller
    {
        // GET: Browse
        public ActionResult Index()
        {
            var users = BrowserRepository.GetAllGithubUsers();
            ViewBag.Users = users;
            return View("Index");
        }
     
        public ActionResult LookupUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return this.View("LookupError");
            }
            var viewModel = BrowserRepository.SetUpUserStructure(username);
            return this.View("LookupUser", viewModel);
        }

        public ActionResult LookupRepo(string username, string repository)
        {
            if (string.IsNullOrEmpty(repository))
            {
                return this.View("LookupError");
            }

            if(!BrowserRepository.PathExists(username, repository))
            {
                ViewBag.ErrorMessage = "Specified repository could not be found";
                return this.View("LookupError");
            }

            var viewModel = BrowserRepository.SetUpSolutionStructure(username, repository, "");
            if (!BrowserRepository.IsRepositoryReady(username, repository))
            {
                return View("AwaitLookup", "_BrowseLayout", viewModel);
            }
            else
            {
                ViewBag.TreeView = loadTreeView(username, repository);
                return View("LookupFolder", "_BrowseLayout", viewModel);
            }
        }

        public ActionResult LookupFolder(string username, string repository, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this.View("LookupError");
            }

            if (!BrowserRepository.PathExists(username, repository, path))
            {
                ViewBag.ErrorMessage = "Specified folder could not be found";
                return this.View("LookupError");
            }

            ViewBag.TreeView = loadTreeView(username, repository);
            var viewModel = BrowserRepository.SetUpSolutionStructure(username, repository, path);
            return View("LookupFolder", "_BrowseLayout", viewModel);
        }

        public ActionResult LookupFile(string username, string repository, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return View("LookupError");
            }

            if (!BrowserRepository.FileExists(username, repository, path))
            {
                ViewBag.ErrorMessage = "Specified file could not be found";
                return this.View("LookupError");
            }
            var rawHtml = BrowserRepository.GetDocumentHtml(username, repository, path);

            ViewBag.TreeView = loadTreeView(username, repository);
            var viewModel = BrowserRepository.SetUpFileStructure(username, repository, path, rawHtml);
            return View("LookupFile", "_BrowseLayout", viewModel);
        }

        public ActionResult LookupFileAjax(string username, string repository, string path)
        {
            var rawHtml = BrowserRepository.GetDocumentHtml(username, repository, path);

            ViewBag.TreeView = loadTreeView(username, repository);
            var viewModel = BrowserRepository.SetUpFileStructure(username, repository, path, rawHtml);
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        private dynamic loadTreeView(string username, string repository)
        {
            var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\";
            string treeViewFileName = "treeview.html";
            var treeViewPath = Path.Combine(organizationPath, username, repository, treeViewFileName);

            var treeViewFile = new StreamReader(treeViewPath);
            string treeViewString = treeViewFile.ReadToEnd();
            treeViewFile.Close();

            return treeViewString;
        }
    }
}