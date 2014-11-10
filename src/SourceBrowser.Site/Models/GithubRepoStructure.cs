namespace SourceBrowser.Site.Models
{
    using System.Collections.Generic;

    public class GithubRepoStructure
    {
        public string Name { get; set; }

        public IList<string> Solutions { get; set; }

        public GithubUserStructure ParentUser { get; set; }

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