using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Token
    {
        public string FullName { get; set; }

        //TODO: Change to enum.
        public string Type { get; set; }

        public string Value { get; set; }

        public IEnumerable<ILink> Links { get; set; }

        public Token()
        {
            Links = new List<ILink>();
        }
    }
}
