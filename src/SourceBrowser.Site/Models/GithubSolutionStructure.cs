using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SourceBrowser.Site.Models
{
    public class GithubSolutionStructure
    {
        public string Name;
        public string RelativePath;
        public string RelativeRootPath;
        public JObject SolutionInfo;
        public GithubRepoStructure ParentRepo;

        public override string ToString()
        {
            return String.Format("'{0}' from {1}'s repo {2}", new object[]
                {
                    Name,
                    ParentRepo.ParentUser.Username,
                    ParentRepo.Name
                });
        }
    }
}