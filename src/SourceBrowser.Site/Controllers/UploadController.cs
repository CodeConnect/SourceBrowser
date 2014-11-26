namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;

    using SourceBrowser.SolutionRetriever;
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
                ViewBag.Error = "Make sure that the provided path is valid.";
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
                var filenamePosition = path.LastIndexOf('\\');
                var solutionName = path.Substring(filenamePosition);
                var solutionPath = repoPath + solutionName; // don't use Path.Combine because solutionName contains "\"
                var sourceGenerator = new Generator.SolutionAnalayzer(path);
                sourceGenerator.AnalyzeAndSave(solutionPath);
            }

            return RedirectToAction("LookupFolder", "Browse", new { id = retriever.UserName + "/" + retriever.RepoName });
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