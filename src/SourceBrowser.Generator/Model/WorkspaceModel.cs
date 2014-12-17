using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkspaceModel : IProjectItem
    {
        public ICollection<IProjectItem> Children { get; }

        public string Name { get; }

        public string BasePath { get; }

        public string RelativePath { get; }

        //The WorkspaceModel has no parent. It is the top level item.
        public IProjectItem Parent
        {
            get
            {
                return null;
            }
        }

        public WorkspaceModel(string name, string basePath)
        {
            Name = name;
            BasePath = basePath;
            RelativePath = "";
            Children = new List<IProjectItem>();
        }
    }
}
