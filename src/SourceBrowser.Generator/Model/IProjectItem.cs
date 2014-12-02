using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    /// <summary>
    /// Represents an item within a workspace.
    /// </summary>
    public interface IProjectItem
    {
        IProjectItem Parent { get; }
        
        string Name { get; }

        ICollection<IProjectItem> Children { get; }

        string RelativePath { get; }
    }
}
