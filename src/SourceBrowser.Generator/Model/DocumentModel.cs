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

        public string RelativePath { get; set; }

        public int NumberOfLines { get; set; }

        public DocumentModel(IProjectItem parent, string name, string relativePath, int NumberOfLines)
        {
            Parent = parent;
            Name = name;
            RelativePath = relativePath;
            NumberOfLines = NumberOfLines;
            Children = new List<IProjectItem>();
            Tokens = new List<Token>();
        }
    }
}
