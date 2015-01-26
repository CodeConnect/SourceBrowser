using System;
using Octokit;
using System.Configuration;

namespace SourceBrowser.SolutionRetriever
{
    public static class GitHubInformationRetriever
    {
        private static GitHubClient _github = new GitHubClient(new ProductHeaderValue("SourceBrowser"));

        static GitHubInformationRetriever()
        {
            // Try to authenticate so that we can make 5000 API calls /hr instead of 60
            // See: https://developer.github.com/v3/#rate-limiting
            string token = ConfigurationManager.AppSettings["GitHubBasicAuthenticationToken"];
            if (!String.IsNullOrEmpty(token))
            {
                _github.Connection.Credentials = new Credentials(token);
            }
        }

        public static void GetUserInformation(string username, ref string fullName, ref string avatarUrl, ref string gitHubUrl, ref string blogUrl)
        {
            var user = _github.User.Get(username).Result;

            avatarUrl = user.AvatarUrl;
            gitHubUrl = user.HtmlUrl;
            blogUrl = user.Blog;
            fullName = user.Name;
        }

        public static void GetRepoInformation(string userName, string repoName, ref int forksCount, ref int starsCount, ref string language, ref string homepage, ref bool isPrivate, ref string description)
        {
            var repo = _github.Repository.Get(userName, repoName).Result;

            forksCount = repo.ForksCount;
            starsCount = repo.StargazersCount;
            language = repo.Language;
            homepage = repo.Homepage;
            isPrivate = repo.Private;
            description = repo.Description;
        }

    }
}
