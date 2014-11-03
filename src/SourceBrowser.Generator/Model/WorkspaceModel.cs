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
    public class WorkspaceModel
    {
        public ICollection<IProjectItem> Children { get; set; }
    }
}
