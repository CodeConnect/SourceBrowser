namespace SourceBrowser.Site.Models
{
    using System;

    using Newtonsoft.Json.Linq;

    public class GithubFileStructure
    {
        public string FileName { get; set; }

        public string Directory { get; set; }

        public string RelativePath { get; set; }

        public string RelativeRootPath { get; set; }

        public string SourceCode { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0} in {1}",
                new object[]
                    {
                        FileName,
                    Directory
                });
        }
    }
}