using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceBrowser.Site.Models
{
    public class GithubRepoStructure
    {
        public string Name;
        public IList<string> Solutions;
        public GithubUserStructure ParentUser;

        public override string ToString()
        {
            return String.Format("{0}'s repository {1} with {2} solution{3}", new object[]
                {
                    ParentUser.Username,
                    Name, 
                    Solutions.Count, 
                    Solutions.Count == 1 ? "" : "s"
                });
        }
    }
}