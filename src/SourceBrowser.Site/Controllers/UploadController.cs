namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;
    using SourceBrowser.SolutionRetriever;
    using SourceBrowser.Generator.Transformers;
    using SourceBrowser.Site.Repositories;
    using System;
    using System.Linq;

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

                // Generate the source browser files for this solution
                var solutionPaths = GetSolutionPaths(repoRootPath);
                if (solutionPaths.Length == 0)
                {
                    ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                    return View("Index");
                }

                var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\" + retriever.UserName;
                var repoPath = Path.Combine(organizationPath, retriever.RepoName);

                // TODO: Use parallel for.
                // TODO: Process all solutions.
                // For now, we're assuming the shallowest and shortest .sln file is the one we're interested in
                foreach (var solutionPath in solutionPaths.OrderBy(n => n.Length).Take(1))
                {
                    try
                    {
                        var workspaceModel = UploadRepository.ProcessSolution(solutionPath, repoRootPath);

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
                    }
                    catch (Exception ex)
                    {
                        // TODO: Log this
                        ViewBag.Error = "There was an error processing solution " + Path.GetFileName(solutionPath);
                        return View("Index");
                    }
                }

                try
                {
                    UploadRepository.SaveReadme(repoPath, retriever.ProvideParsedReadme());
                }
                catch (Exception ex)
                {
                    // TODO: Log and swallow - readme is not essential.
                }

                processingSuccessful = true;
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

        /// <summary>
        /// Simply searches for the solution files and returns their paths.
        /// </summary>
        /// <param name="rootDirectory">
        /// The root Directory.
        /// </param>
        /// <returns>
        /// The solution paths.
        /// </returns>
        private string[] GetSolutionPaths(string rootDirectory)
        {
            return Directory.GetFiles(rootDirectory, "*.sln", SearchOption.AllDirectories);
        }
    }
}