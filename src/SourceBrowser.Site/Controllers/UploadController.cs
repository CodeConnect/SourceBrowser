namespace SourceBrowser.Site.Controllers
{
    using System.IO;
    using System.Web.Mvc;
    using SourceBrowser.SolutionRetriever;
    using SourceBrowser.Generator.Transformers;
    using SourceBrowser.Site.Repositories;
    using System;
    using System.Linq;
    using System.Web.Routing;

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
                // Repo exists. Redirect the user to the Update view
                // where he can choose if he wants to visit the existing repo or update it.
                var routeValues = new RouteValueDictionary();
                routeValues.Add(nameof(githubUrl), githubUrl);
                return RedirectToAction("Update", routeValues);
            }

            // We have locked the repository and marked it as processing.
            // Whenever we return or exit on an exception, we need to unlock this repository
            bool processingSuccessful = false;
            try
            {
                string repoSourceStagingPath = null;

                try
                {
                    repoSourceStagingPath = retriever.RetrieveProject();
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "There was an error downloading this repository.";
                    return View("Index");
                }

                // Generate the source browser files for this solution

                var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\" + retriever.UserName;
                var parsedRepoPath = Path.Combine(organizationPath, retriever.RepoName);

                try
                {
                    processingSuccessful = UploadRepository.ProcessRepo(retriever, repoSourceStagingPath, parsedRepoPath);
                }
                catch (NoSolutionsFoundException)
                {
                    ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                    return View("Index");
                }
                catch (Exception ex)
                {
                    // TODO: Log this
                    ViewBag.Error = "There was an error processing solution."/* + Path.GetFileName(solutionPath)*/;
                    return View("Index");
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

        /// <summary>
        /// This API is used for the updating an existing repo.
        /// If the request's http method is GET, it will return a View with a link to the existing repo
        /// and a form that allows the user to force the update.
        /// If the request's http method is POST, it will delete the existing repo and then
        /// it will process the github url.
        /// </summary>
        /// <param name="githubUrl">The github url of the repo.</param>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Update(string githubUrl)
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
            if (!BrowserRepository.PathExists(retriever.UserName, retriever.RepoName))
            {
                // Repo doesn't exist.
                ViewBag.Error = $"The repo \"{githubUrl}\" cannot be updated because it was never submitted.";
                return View("Index");
            }

            string currentHttpMethod = HttpContext.Request.HttpMethod.ToUpper();

            if (currentHttpMethod == HttpVerbs.Get.ToString().ToUpper())
            {
                ViewBag.BrowseUrl = "/Browse/" + retriever.UserName + "/" + retriever.RepoName;
                ViewBag.GithubUrl = githubUrl;
                return View("Update");
            }
            else if (currentHttpMethod == HttpVerbs.Post.ToString().ToUpper())
            {
                // Remove old files
                BrowserRepository.RemoveRepository(retriever.UserName, retriever.RepoName);
                // Create a file that indicates that the upload will begin
                BrowserRepository.TryLockRepository(retriever.UserName, retriever.RepoName);

                // We have locked the repository and marked it as processing.
                // Whenever we return or exit on an exception, we need to unlock this repository
                bool processingSuccessful = false;
                try
                {
                    string repoSourceStagingPath = null;

                    try
                    {
                        repoSourceStagingPath = retriever.RetrieveProject();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = "There was an error downloading this repository.";
                        return View("Index");
                    }

                    var organizationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "SB_Files\\" + retriever.UserName;
                    var parsedRepoPath = Path.Combine(organizationPath, retriever.RepoName);

                    try
                    {
                        // Generate the source browser files for this solution
                        processingSuccessful = UploadRepository.ProcessRepo(retriever, repoSourceStagingPath, parsedRepoPath);
                    }
                    catch (NoSolutionsFoundException)
                    {
                        ViewBag.Error = "No C# solution was found. Ensure that a valid .sln file exists within your repository.";
                        return View("Index");
                    }
                    catch (Exception ex)
                    {
                        // TODO: Log this
                        ViewBag.Error = "There was an error processing solution."/* + Path.GetFileName(solutionPath)*/;
                        return View("Index");
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
            else
            {
                // The request's http method wasn't valid: back to index 
                ViewBag.Error = $"Bad request.";
                return View("Index");
            }
        }
    }
}