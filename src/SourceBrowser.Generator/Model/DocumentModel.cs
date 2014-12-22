using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class DocumentModel : IProjectItem
    {
        public IProjectItem Parent { get; }

        public ICollection<IProjectItem> Children { get; }

        public ICollection<Token> Tokens { get;  }

        public string Name { get; }

        public string RelativePath { get; }

        public int NumberOfLines { get; } 

        public DocumentModel(IProjectItem parent, string name, int numberOfLines)
        {
            Parent = parent;
            Name = name;
            RelativePath = Path.Combine(parent.RelativePath, name); ;
            NumberOfLines = numberOfLines;
            Children = new List<IProjectItem>();
            Tokens = new List<Token>();
        }
    }
}
