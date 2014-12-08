using System;
using System.IO;
using Microsoft.CodeAnalysis;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Generator.DocumentWalkers
{
    class WalkerSelector
    {
        public static IWalker GetWalker(IProjectItem parent, Document document, ReferencesourceLinkProvider refSourceLinkProvider)
        {
            var fileExtension = Path.GetExtension(document.FilePath);
            if (String.IsNullOrEmpty(fileExtension))
            {
                return null;
            }
            if (fileExtension == ".cs")
            {
                return new DocumentWalker(parent, document, refSourceLinkProvider);
            }
            /*
            else if (fileExtension == ".vb")
            {
                return new DocumentWalker(parent, document, refSourceLinkProvider);
            }*/
            return null;
        }
    }
}
