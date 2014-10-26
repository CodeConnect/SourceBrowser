using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Document : IProjectItem
    {
        public IProjectItem Parent { get; set; }

        public IEnumerable<IProjectItem> Children { get; set; }

        public Document(IProjectItem parent)
        {
            Parent = parent;
        }
    }
}
