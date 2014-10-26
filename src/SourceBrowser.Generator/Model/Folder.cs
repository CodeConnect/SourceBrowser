using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Folder : IProjectItem
    {
        public IEnumerable<IProjectItem> Children { get; set; }

        public IProjectItem Parent { get; private set; }

        public Folder(IProjectItem parent)
        {
            Parent = parent;
        }
    }
}
