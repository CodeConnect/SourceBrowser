namespace SourceBrowser.Site.Models
{
    using SourceBrowser.SolutionRetriever;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class GithubRepoStructure
    {
        public string Name { get; set; }

        public string ParentUserName { get; set; }

        public DateTime UploadTime { get; set; } // todo: populate.

        public int forksCount;
        public int starsCount;
        public string language;
        public string homepage;
        public bool isPrivate;
        public string description;

        public void UseLiveData()
        {
            try
            {
                GitHubInformationRetriever.GetRepoInformation(ParentUserName, Name, ref forksCount, ref starsCount, ref language, ref homepage, ref isPrivate, ref description);
            }
            catch
            {
                // Swallow. This information is not essential to operation of SourceBrowser.
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0}'s repository {1}",
                ParentUserName,
                Name);
        }
    }
}