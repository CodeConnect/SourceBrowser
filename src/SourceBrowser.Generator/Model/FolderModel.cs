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

        public string Name { get; set; }

        public FolderModel(IProjectItem parent, string name)
        {
            Parent = parent;
            Name = name;
            Children = new List<IProjectItem>();    
        }

        public string GetPath()
        {
            IProjectItem currentNode = this;
            string path = "//" + this.Name;
            while(!(currentNode.Parent is WorkspaceModel))
            {
                path = "//" + currentNode.Parent.Name + path;
                currentNode = currentNode.Parent;
            }

            return path;
        }
    }
}
