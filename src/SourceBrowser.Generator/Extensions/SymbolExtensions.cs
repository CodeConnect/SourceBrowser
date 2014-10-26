using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SourceBrowser.Generator.Extensions
{
    public static class SymbolExtensions
    {

        public static string GetSymbolId(this ISymbol symbol)
        {
            string documentationCommentId = String.Empty;
            symbol = symbol.OriginalDefinition;
            documentationCommentId = symbol.GetDocumentationCommentId();

            documentationCommentId = documentationCommentId.Replace("#ctor", "ctor");

            return documentationCommentId;
        }
    }
}
