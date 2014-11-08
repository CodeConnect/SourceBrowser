using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SourceBrowser.Site.Models
{
    /// <summary>
    /// Contains basic information about a source file
    /// to be displayed within a view.
    /// </summary>
    public class SourceFileViewModel
    {
        public string SourceCode { get; set; }
        public string FileName { get; set; }
        public string LookupPath { get; set; }
        public int NumberOfLines { get; set; }
        public JObject FolderInfoRoot { get; set; }

        public string RootDirectory { get; set; }

    }
}