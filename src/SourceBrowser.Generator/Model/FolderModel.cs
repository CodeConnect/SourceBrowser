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

        public string RelativePath { get; }

        public FolderModel(IProjectItem parent, string name, string path)
        {
            Parent = parent;
            Name = name;
            RelativePath = findRelativePath(path);
            Children = new List<IProjectItem>();    
        }

        private string findRelativePath(string path)
        {
            //Find the root WorkspaceModel
            IProjectItem currentNode = this;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
            }

            string rootPath = ((WorkspaceModel)currentNode).RelativePath;
            var relativePath = path.Remove(0, rootPath.Length);

            return relativePath;
        }
    }
}
