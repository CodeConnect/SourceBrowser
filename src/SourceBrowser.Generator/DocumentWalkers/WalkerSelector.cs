using System;
using System.IO;
using Microsoft.CodeAnalysis;
using SourceBrowser.Generator.Model;


namespace SourceBrowser.Generator.DocumentWalkers
{
    internal class WalkerSelector
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
                return new DocumentWalker<CSWalkerUtils>(parent, document, refSourceLinkProvider, (w) => new CSWalkerUtils(w));
            }
            else if (fileExtension == ".vb")
            {
                return new DocumentWalker<VBWalkerUtils>(parent, document, refSourceLinkProvider, (w) => new VBWalkerUtils(w));
            }
            return null;
        }
    }
}
