using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class DocumentModel : IProjectItem
    {
        public IProjectItem Parent { get; set; }

        public IEnumerable<IProjectItem> Children { get; set; }

        public string Name { get; set; }

        public DocumentModel(IProjectItem parent)
        {
            Parent = parent;
        }
    }
}
