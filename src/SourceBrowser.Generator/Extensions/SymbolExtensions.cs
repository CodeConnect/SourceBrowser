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

        /// <summary>
        /// Returns the SymbolId for a symbol. SymbolId is very closely 
        /// related to DocumentationCommentId, with some minor changes.
        /// </summary>
        public static string GetSymbolId(this ISymbol symbol)
        {
            string documentationCommentId = String.Empty;
            symbol = symbol.OriginalDefinition;
            documentationCommentId = symbol.GetDocumentationCommentId();
            if (documentationCommentId == null)
            {
                return null;
            }
            documentationCommentId = documentationCommentId.Replace("#ctor", "ctor");

            return documentationCommentId;
        }
    }
}
