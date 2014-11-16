namespace SourceBrowser.Site.Models
{
    using SourceBrowser.SolutionRetriever;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class GithubRepoStructure
    {
        public string Name { get; set; }

        public IList<string> Solutions { get; set; }

        public GithubUserStructure ParentUser { get; set; }

        public int forksCount;
        public int starsCount;
        public string language;
        public string homepage;
        public bool isPrivate;

        public void UseLiveData()
        {
            GitHubInformationRetriever.GetRepoInformation(ParentUser.Username, Name, ref forksCount, ref starsCount, ref language, ref homepage, ref isPrivate);
        }

        public override string ToString()
        {
            return string.Format(
                "{0}'s repository {1} with {2} solution{3}",
                ParentUser.Username,
                Name,
                Solutions.Count,
                Solutions.Count == 1 ? "" : "s");
        }
    }
}