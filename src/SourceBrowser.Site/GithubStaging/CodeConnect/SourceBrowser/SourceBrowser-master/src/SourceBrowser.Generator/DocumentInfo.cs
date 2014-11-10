using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator
{
    [Serializable]
    public struct DocumentInfo
    {
        public int NumberOfLines { get; set; }
        public string FileName { get; set; }
        public string HtmlContent { get; set; }
    }
}
