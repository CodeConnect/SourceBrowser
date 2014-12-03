using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class FolderModel : IProjectItem
    {
        public ICollection<IProjectItem> Children { get; set; }

        public IProjectItem Parent { get; private set; }

        public string Name { get; set; }

        public string RelativePath { get; }

        public FolderModel(IProjectItem parent, string name)
        {
            Parent = parent;
            Name = name;
            RelativePath = Path.Combine(parent.RelativePath, name) + "/";
            Children = new List<IProjectItem>();    
        }
    }
}
