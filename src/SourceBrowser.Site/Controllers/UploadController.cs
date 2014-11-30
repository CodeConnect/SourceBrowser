namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;

    using SourceBrowser.SolutionRetriever;
    using SourceBrowser.Generator.Transformers;

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
                // TODO: Return error
                ViewBag.Error = "Invalid GitHub repository. Please try another";
                return View("Index");
            }

            string filePath = retriever.RetrieveProject();
            var repoPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/"), "SB_Files", retriever.UserName, retriever.RepoName);

            // Generate the source browser files for this solution
            var solutionPaths = GetSolutionPaths(filePath);
            if (solutionPaths.Length == 0)
            {
                ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                return View("Index");
            }

            // TODO: Use parallel for.
            foreach (var path in solutionPaths)
            {
                System.Diagnostics.Debug.WriteLine("path: ", path);
                var solutionName = Path.GetFileName(path);
                var solutionPath = Path.Combine(repoPath, solutionName); 
                System.Diagnostics.Debug.WriteLine("soln path: ", solutionPath);

                var sourceGenerator = new Generator.SolutionAnalayzer(path);
                //Build the workspace
                var workspaceModel = sourceGenerator.BuildWorkspaceModel(solutionPath);

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