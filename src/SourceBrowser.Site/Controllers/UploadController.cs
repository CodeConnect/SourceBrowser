namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;
    using SourceBrowser.SolutionRetriever;
    using SourceBrowser.Generator.Transformers;
    using SourceBrowser.Site.Repositories;
    using System;
    using System.Linq;
    using Generator.Model;
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Submit(string githubUrl)
        {
            // If someone navigates to submit directly, just send 'em back to index
            if (string.IsNullOrWhiteSpace(githubUrl))
            {
                return View("Index");
            }

            var retriever = new GitHubRetriever(githubUrl);
            if (!retriever.IsValidUrl())
            {
                ViewBag.Error = "Make sure that the provided path points to a valid GitHub repository.";
                return View("Index");
            }

            // Check if this repo already exists
            if (!BrowserRepository.TryLockRepository(retriever.UserName, retriever.RepoName))
            {
	            // Repo exists. Redirect the user to that repository.
	            return Redirect("/Browse/" + retriever.UserName + "/" + retriever.RepoName);
            }
            // We have locked the repository and marked it as processing.
            // Whenever we return or exit on an exception, we need to unlock this repository
            bool processingSuccessful = false;
            try
            {
                string repoRootPath = string.Empty;
                try
                {
                    repoRootPath = retriever.RetrieveProject();
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "There was an error downloading this repository.";
                    return View("Index");
                }

                var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\" + retriever.UserName;
                var repoPath = Path.Combine(organizationPath, retriever.RepoName);


                // Generate the source browser files for this solution
                var solutionPaths = Directory.GetFiles(repoRootPath, "*.sln", SearchOption.AllDirectories);
                var omnisharpPaths = Directory.GetFiles(repoRootPath, "omnisharp.json", SearchOption.AllDirectories);
                var projectJsonPaths= Directory.GetFiles(repoRootPath, "project.json", SearchOption.AllDirectories);

                string solutionPath = solutionPaths.OrderBy(n => n.Length).FirstOrDefault();
                string omnisharpPath = omnisharpPaths.OrderBy(n => n.Length).FirstOrDefault();

                WorkspaceModel workspaceModel = null;
                if(solutionPath != null)
                {
                    workspaceModel = UploadRepository.ProcessSolution(solutionPath, repoRootPath);
                }

                if(workspaceModel == null && omnisharpPath != null)
                {
                    workspaceModel = UploadRepository.ProcessOmnisharp(omnisharpPath, repoRootPath);
                }

                if(workspaceModel == null && projectJsonPaths.Count() > 0)
                {
                    workspaceModel = UploadRepository.ProcessProjectJson(projectJsonPaths, repoRootPath);
                }

                if (workspaceModel != null)
                {
                    //One pass to lookup all declarations
                    var typeTransformer = new TokenLookupTransformer();
                    typeTransformer.Visit(workspaceModel);
                    var tokenLookup = typeTransformer.TokenLookup;

                    //Another pass to generate HTMLs
                    var htmlTransformer = new HtmlTransformer(tokenLookup, repoPath);
                    htmlTransformer.Visit(workspaceModel);

                    var searchTransformer = new SearchIndexTransformer(retriever.UserName, retriever.RepoName);
                    searchTransformer.Visit(workspaceModel);

                    // Generate HTML of the tree view
                    var treeViewTransformer = new TreeViewTransformer(repoPath, retriever.UserName, retriever.RepoName);
                    treeViewTransformer.Visit(workspaceModel);
                    processingSuccessful = true;
                }

                return Redirect("/Browse/" + retriever.UserName + "/" + retriever.RepoName);
            }
            finally
            {
                if (processingSuccessful)
                {
                    BrowserRepository.UnlockRepository(retriever.UserName, retriever.RepoName);
                }
                else
                {
                    BrowserRepository.RemoveRepository(retriever.UserName, retriever.RepoName);
                }
            }
        }
    }
}