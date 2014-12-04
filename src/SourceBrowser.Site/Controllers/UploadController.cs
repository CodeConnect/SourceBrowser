namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;
    using SourceBrowser.SolutionRetriever;
    using SourceBrowser.Generator.Transformers;
    using System;

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

            string filePath = string.Empty;
            try
            {
                filePath = retriever.RetrieveProject();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "There was an error downloading this repository.";
                return View("Index");
            }

            // Generate the source browser files for this solution
            var solutionPaths = GetSolutionPaths(filePath);
            if (solutionPaths.Length == 0)
            {
                ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                return View("Index");
            }

            var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\" + retriever.UserName;
            var repoPath = Path.Combine(organizationPath, retriever.RepoName);

            // TODO: Use parallel for.
            foreach (var path in solutionPaths)
            {
                var solutionName = Path.GetFileName(path);
                var modelSavePath = Path.Combine(repoPath, solutionName); 
                var sourceGenerator = new Generator.SolutionAnalayzer(path);

                //Build the workspace
                var workspaceModel = sourceGenerator.BuildWorkspaceModel(filePath);

                //One pass to lookup all declarations
                var typeTransformer = new TokenLookupTransformer();
                typeTransformer.Visit(workspaceModel);
                var tokenLookup = typeTransformer.TokenLookup;

                //Another pass to generate HTMLs
                var htmlTransformer = new HtmlTransformer(tokenLookup, repoPath);
                htmlTransformer.Visit(workspaceModel);

                // Generate HTML of the tree view
                var treeViewTransformer = new TreeViewTransformer(repoPath, retriever.UserName, retriever.RepoName);
                treeViewTransformer.Visit(workspaceModel);
            }

            return Redirect("/Browse/" + retriever.UserName + "/" + retriever.RepoName);
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