using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class FolderModel : IProjectItem
    {
        public ICollection<IProjectItem> Children { get; set; }

        public IProjectItem Parent { get; private set; }

        public FolderModel(IProjectItem parent)
        {
            Parent = parent;
            Children = new List<IProjectItem>();
        }
    }
}
