namespace SourceBrowser.Site.Controllers
{
    using System;
    using System.Web.Mvc;

    using SourceBrowser.Site.Repositories;
    using System.IO;

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

            var viewModel = BrowserRepository.SetUpRepoStructure(username, repository);
            return this.View("LookupRepo", viewModel);
        }

        public ActionResult LookupFolder(string username, string repository, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this.View("LookupError");
            }

            ViewBag.TreeView = loadTreeView(username, repository);
            var viewModel = BrowserRepository.SetUpSolutionStructure(username, repository, path);
            return View("LookupFolder", viewModel);
        }

        public ActionResult LookupFile(string username, string repository, string path)
        {
            System.Diagnostics.Debug.Write(username);
            System.Diagnostics.Debug.Write(repository);
            System.Diagnostics.Debug.Write(path);
            if (string.IsNullOrEmpty(path))
            {
                return View("LookupError");
            }

            var metaData = BrowserRepository.GetMetaData(username, repository, path);
            int numberOfLines = metaData["NumberOfLines"].ToObject<int>();
            var rawHtml = BrowserRepository.GetDocumentHtml(username, repository, path);

            ViewBag.TreeView = loadTreeView(username, repository);
            var viewModel = BrowserRepository.SetUpFileStructure(username, repository, path, rawHtml, numberOfLines);
            return View("LookupFile", viewModel);
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