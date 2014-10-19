using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator
{
    /// <summary>
    /// Type Info retains various information about a type and it's 
    /// declaration location. This will allow us to create links that point to it.
    /// </summary>
    public struct TypeInfo
    {
        int LineNumber { get; set; }
        string FullyQualifiedName { get; set; }
        string Url { get; set; }
    }
}
