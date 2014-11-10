using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Octokit;
using System.IO;

namespace SourceBrowser.SolutionRetriever
{
    public class GitHubRetriever
    {
        private string _url;
        private Guid guid = Guid.NewGuid();
        //private string _userName = String.Empty;
        public string UserName { get; set; }
        //private string _repoName = string.Empty;
        public string RepoName { get; set; }

        private string _downloadUrl
        {
            get
            {
                //TODO: retrieve branches instead of assuming
                //Master exists
                return _url + "/archive/master.zip";
            }
        }

        public GitHubRetriever(string url)
        {
            _url = url;
            var splitUrl = url.Split('/');

            if (splitUrl.Length < 3)
                return;

            UserName = splitUrl[splitUrl.Length - 2];
            RepoName = splitUrl[splitUrl.Length - 1];
        }

        public bool IsValidUrl()
        {
            if (!_url.Contains("github.com"))
                return false;
            if (String.IsNullOrWhiteSpace(UserName))
                return false;
            if (String.IsNullOrWhiteSpace(RepoName))
                return false;

            var githubClient = new GitHubClient(new ProductHeaderValue("SourceBrowser"));
            try
            {
                var repository = githubClient.Repository.Get(UserName, RepoName).Result;
            }
            catch (AggregateException e)
            {
                //Assuming the inner exception is a NotFoundException
                return false;
            }

            return true;
        }

        public string RetrieveProject()
        {
            string baseRepositoryPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "\\GithubStaging\\";
            if (!Directory.Exists(baseRepositoryPath))
                Directory.CreateDirectory(baseRepositoryPath);

            string zipName = guid + ".zip";
            using (var client = new WebClient())
            {
                client.DownloadFile(_downloadUrl, baseRepositoryPath + zipName);
            }
            string absoluteRepositoryPath = baseRepositoryPath + UserName + '\\' + RepoName;
            if (Directory.Exists(absoluteRepositoryPath))
            {
                try
                {
                    Directory.Delete(absoluteRepositoryPath, recursive: true);
                }
                catch
                {
                    // Swallow.
                }
            }

            //unpack the zip
            System.IO.Compression.ZipFile.ExtractToDirectory(baseRepositoryPath + zipName, absoluteRepositoryPath);
            deleteZipFile(baseRepositoryPath);
            return absoluteRepositoryPath;
        }

        private void deleteZipFile(string baseRepoPath)
        {
            string zipName = guid + ".zip";
            System.IO.File.Delete(baseRepoPath + zipName);
        }
    }
}
