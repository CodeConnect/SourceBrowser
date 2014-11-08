using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SourceBrowser.Site.Models
{
    public class GithubFileStructure
    {
        public string FileName;
        public string Directory;
        public string RelativePath;
        public string RelativeRootPath;
        public string SourceCode;
        public int NumberOfLines;
        public JObject SolutionInfo; // folderInfoRoot

        public override string ToString()
        {
            return String.Format("{0} in {1}", new object[]
                {
                    FileName,
                    Directory
                });
        }
    }
}