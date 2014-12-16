using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Octokit;

namespace SourceBrowser.SolutionRetriever
{
    public class GitHubInformationRetriever
    {
        private static GitHubClient _github = new GitHubClient(new ProductHeaderValue("SourceBrowser"));

        public static void GetUserInformation(string username, ref string fullName, ref string avatarUrl, ref string gitHubUrl, ref string blogUrl)
        {
            try
            {
                var user = _github.User.Get(username).Result;

                avatarUrl = user.AvatarUrl;
                gitHubUrl = user.HtmlUrl;
                blogUrl = user.Blog;
                fullName = user.Name;
            }
            catch
            {
                // Swallow. This wasn't necessary information
            }
        }

        public static void GetRepoInformation(string userName, string repoName, ref int forksCount, ref int starsCount, ref string language, ref string homepage, ref bool isPrivate, ref string description)
        {
            try
            {
                var repo = _github.Repository.Get(userName, repoName).Result;

                forksCount = repo.ForksCount;
                starsCount = repo.StargazersCount;
                language = repo.Language;
                homepage = repo.Homepage;
                isPrivate = repo.Private;
                description = repo.Description;
            }
            catch
            {
                // Swallow. This wasn't necessary information
            }
        }

    }
}
