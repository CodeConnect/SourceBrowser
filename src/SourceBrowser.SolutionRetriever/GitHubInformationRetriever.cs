using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace SourceBrowser.SolutionRetriever
{
    public class GitHubInformationRetriever : IInformationRetriever
    {
        // TODO: Figure out the best location for the GitHubInformationRetriever class
        // This also exists in SourceBrowser.Site.Repositories.BrowseRepository
        private static readonly string ProjectAbsolutePath = System.Web.Hosting.HostingEnvironment.MapPath("~/SB_Files/");

        #region IInformationRetriever implementation

        public bool TryUpdateUserInformation(string userName)
        {
            string userInformationPath = Path.Combine(ProjectAbsolutePath, userName, "user.json");
            if (fileIsFresh(userInformationPath))
            {
                return false;
            }
            updateUserInformation(userName, userInformationPath);
            return true;
        }

        public bool TryUpdateRepoInformation(string userName, string repoName)
        {
            string repoInformationPath = Path.Combine(ProjectAbsolutePath, userName, repoName, "repo.json");
            if (fileIsFresh(repoInformationPath))
            {
                return false;
            }
            updateRepoInformation(userName, repoName, repoInformationPath);
            return true;
        }

        public JObject GetUserInformation(string userName)
        {
            string userInformationPath = Path.Combine(ProjectAbsolutePath, userName, "user.json");
            return JObject.Parse(File.ReadAllText(userInformationPath));
        }

        public JObject GetRepoInformation(string userName, string repoName)
        {
            string repoInformationPath = Path.Combine(ProjectAbsolutePath, userName, repoName, "repo.json");
            return JObject.Parse(File.ReadAllText(repoInformationPath));
        }

        #endregion

        private void updateUserInformation(string userName, string userInformationPath)
        {
            Uri requestURL = new Uri("https://api.github.com/users/" + userName);
            using (var webClient = new WebClient())
            {
                webClient.DownloadFileAsync(requestURL, userInformationPath);
            }
        }

        private void updateRepoInformation(string userName, string repoName, string repoInformationPath)
        {
            Uri requestURL = new Uri("https://api.github.com/repos/" + userName + "/" + repoName);
            using (var webClient = new WebClient())
            {
                webClient.DownloadFileAsync(requestURL, repoInformationPath);
            }
        }

        private bool fileIsFresh(string filePath)
        {
            // If file doesn't exist, it's not "fresh" (it needs to be downloaded)
            if (!File.Exists(filePath))
            {
                return false;
            }

            // If file has been modified within last 60 minutes, it is considered "fresh"
            DateTime freshThreshold = DateTime.Now - TimeSpan.FromMinutes(60);
            if (File.GetLastWriteTimeUtc(filePath) > freshThreshold.ToUniversalTime())
            {
                return true;
            }

            return false;
        }
    }
}
