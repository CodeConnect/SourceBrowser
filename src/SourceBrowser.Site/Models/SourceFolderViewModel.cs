namespace SourceBrowser.Site.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;

    public class SourceFolderViewModel
    {
        public string FolderName { get; set; }

        public string LookupPath { get; set; }

        public List<string> Folders { get; set; }

        public List<string> Files { get; set; }

        public JObject FolderInfoRoot { get; set; }

        public string RootDirectory { get; set; }
    }
}