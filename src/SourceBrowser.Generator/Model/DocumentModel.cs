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

        public ICollection<IProjectItem> Children { get; set; }

        public ICollection<Token> Tokens { get; set; }

        public string Name { get; set; }

        public string ContainingPath { get; set; }

        public int NumberOfLines { get; set; }

        public DocumentModel(IProjectItem parent, string name, string containingPath, int numberOfLines)
        {
            Parent = parent;
            Name = name;
            ContainingPath = containingPath;
            NumberOfLines = numberOfLines;
            Children = new List<IProjectItem>();
            Tokens = new List<Token>();
        }

        public string GetPath()
        {
            IProjectItem currentNode = this;
            string path = "//" + this.Name;
            while (!(currentNode.Parent is WorkspaceModel))
            {
                path = "//" + currentNode.Parent.Name + path;
                currentNode = currentNode.Parent;
            }

            return path;
        }
    }
}
