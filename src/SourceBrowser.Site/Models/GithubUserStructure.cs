namespace SourceBrowser.Site.Models
{
    using System;
    using System.Collections.Generic;

    public class GithubUserStructure
    {
        public string Username;
        public string Path;
        public IList<string> Repos;

        public override string ToString()
        {
            return String.Format("User {0} with {1} repo{2}", Username, Repos.Count, Repos.Count == 1 ? "" : "s");
        }
    }
}