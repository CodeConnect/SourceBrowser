using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceBrowser.Site.Models
{
    public class GithubUserStructure
    {
        public string Username;
        public string Path;
        public IList<string> Repos;

        public override string ToString()
        {
            return String.Format("User {0} with {1} repo{2}", new object[]
                {
                    Username,
                    Repos.Count,
                    Repos.Count == 1 ? "" : "s"
                });
        }
    }
}