using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class UrlLink : ILink
    {
        public string Url { get; }

        public string Link
        {
            get
            {
                return Url;
            }
        }

        public UrlLink(string url)
        {
            Url = url;
        }
    }
}
