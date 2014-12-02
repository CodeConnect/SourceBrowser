using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Given a workspace model, provide a lookup from symbols to their position
    /// in a document.
    /// </summary>
    public class TokenLookupTransformer : AbstractWorkspaceVisitor
    {
        public Dictionary<string, Token> TokenLookup;

        public TokenLookupTransformer()
        {
            TokenLookup = new Dictionary<string, Token>();
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            var declarations = documentModel.Tokens.Where(n => n.IsDeclaration);
            foreach(var declaration in declarations)
            {
                TokenLookup[declaration.FullName] = declaration;
            }

            base.VisitDocument(documentModel);
        }
    }
}
