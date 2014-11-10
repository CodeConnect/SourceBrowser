using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Hacks
{
    /// <summary>
    /// Visual Studio will not copy unused references to the /bin folder
    /// of any project that depends on SourceBrowser.Generator.
    /// 
    /// Microsoft.CodeAnalysis loads DLLs via Refletion and expects certain DLLs
    /// to be present in /bin.
    /// 
    /// We tried adding the necessary DLLs directly to the projects that depend on
    /// SourceBrowser.Generator (ie. SourceBrowserSite) but this causes problems with
    /// the MSBuildWorkspace. (It caused 28 LoaderExceptions for us)
    /// 
    /// The workaround seems to be to reference types within the DLLs we want 
    /// copied. (ie. BinaryFormatter for )
    /// </summary>
    class DependencyHacks
    {
        //Copies Microsoft.CodeAnalysis.CSharp.Workspaces.dll
        Microsoft.CodeAnalysis.CSharp.Formatting.BinaryOperatorSpacingOptions workaround1;
    }
}
