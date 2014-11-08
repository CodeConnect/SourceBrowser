using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SourceBrowser.SolutionRetriever;

namespace SourceBrowser.Site.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Submit(string githubUrl)
        {
            //If someone navigates to submit directly, just send 'em back to index
            if (string.IsNullOrWhiteSpace(githubUrl))
                return View("Index");

            var retriever = new GitHubRetriever(githubUrl);
            if (!retriever.IsValidUrl())
            {
                //TODO: Return error
                ViewBag.Error = "Invalid GitHub repository. Please try another";
                return View("Index");
            }

            string filePath = retriever.RetrieveProject();
            var OrganizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/")  + "SB_Files\\" + retriever.UserName;
            var RepoPath = Path.Combine(OrganizationPath, retriever.RepoName);

            //Generate the source browser files for this solution
            var solutionPaths = getSolutionPaths(filePath);
            if (solutionPaths.Length == 0)
            {
                ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                return View("Index");
            }
            // TODO: Use parallel for.
            foreach (var path in solutionPaths)
            {
                var filenamePosition = path.LastIndexOf('\\');
                var solutionName = path.Substring(filenamePosition);
                var SolutionPath = RepoPath + solutionName; // don't use Path.Combine because solutionName contains "\"
                var sourceGenerator = new SourceBrowser.Generator.SolutionAnalayzer(path);
                sourceGenerator.AnalyzeAndSave(SolutionPath);
            }

            return RedirectToAction("LookupFolder", "Browse", new { id = retriever.UserName + "/" + retriever.RepoName });
        }

        /// <summary>
        /// Simply searches for the solution files and returns their paths.
        /// </summary>
        private string[] getSolutionPaths(string rootDirectory)
        {
            return Directory.GetFiles(rootDirectory, "*.sln", SearchOption.AllDirectories);
        }

    }
}