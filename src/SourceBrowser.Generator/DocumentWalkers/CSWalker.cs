using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceBrowser.Generator.Extensions;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator.Model.CSharp;

namespace SourceBrowser.Generator.DocumentWalkers
{
    /// <summary>
    /// 
    /// </summary>
    public class CSWalker : CSharpSyntaxWalker, IWalker
    {
        private SemanticModel _model;
        private ReferencesourceLinkProvider _refsourceLinkProvider;
        public DocumentModel DocumentModel { get; private set; }
        public string FilePath { get; set; }

        public CSWalker(IProjectItem parent, Document document, ReferencesourceLinkProvider refSourceLinkProvider): base(SyntaxWalkerDepth.Trivia)
        {
            _model = document.GetSemanticModelAsync().Result;
            _refsourceLinkProvider = refSourceLinkProvider;
            string containingPath = document.GetRelativeFilePath();

            var numberOfLines = document.GetTextAsync().Result.Lines.Count + 1;
            DocumentModel = new DocumentModel(parent, document.Name, numberOfLines);
            FilePath = document.GetRelativeFilePath();
            _refsourceLinkProvider = refSourceLinkProvider;
        }

        public override void VisitToken(SyntaxToken token)
        {
            string str = String.Empty;
            Token tokenModel = null;
          
            if (token.IsKeyword())
            {
                tokenModel = ProcessKeyword(token);
            }
            else if (token.CSharpKind() == SyntaxKind.IdentifierToken)
            {
                tokenModel = ProcessIdentifier(token);
            }
            else if(token.CSharpKind() == SyntaxKind.StringLiteralToken)
            {
                tokenModel = ProcessStringLiteral(token);
            }
            else
            {
                //This covers all semantically useless tokens such as punctuation
                tokenModel = ProcessOtherToken(token);
            }

            var leadingTrivia = ProcessTrivia(token.LeadingTrivia);
            var trailingTrivia = ProcessTrivia(token.TrailingTrivia);

            //Add trivia to the token
            tokenModel = tokenModel.WithTrivia(leadingTrivia, trailingTrivia);

            DocumentModel.Tokens.Add(tokenModel);
        }

        public DocumentModel GetDocumentModel()
        {
            return DocumentModel;
        }

        private ICollection<Trivia> ProcessTrivia(SyntaxTriviaList triviaList)
        {
            var triviaModelList = triviaList.Select(n => new Trivia(
                value: n.ToFullString(),
                type: n.CSharpKind().ToString()
            )).ToList();

            return triviaModelList;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for non-keywords and non-identifiers.
        /// </summary>
        private Token ProcessOtherToken(SyntaxToken token)
        {
            string fullName = token.CSharpKind().ToString();
            string value = token.ToString();
            string type = CSharpTokenTypes.OTHER;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for a Keyword.
        /// </summary>
        public Token ProcessKeyword(SyntaxToken token)
        {
            string fullName = token.CSharpKind().ToString();
            string value = token.ToString();
            string type = CSharpTokenTypes.KEYWORD;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        private Token ProcessStringLiteral(SyntaxToken token)
        {
            string fullName = token.CSharpKind().ToString();
            string value = token.ToString();
            string type = CSharpTokenTypes.STRING;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        /// <summary>
        /// Given a syntax token identifier that represents a declaration,
        /// generate and return the proper HTML for this symbol.
        /// </summary>
        public Token ProcessDeclarationToken(SyntaxToken token, ISymbol parentSymbol)
        {
            string fullName = parentSymbol.ToString();
            string value = token.ToString();
            string type = string.Empty;
            bool isDeclaration = true;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                 
            if (parentSymbol is INamedTypeSymbol)
            {
                type = CSharpTokenTypes.TYPE;
            }
            else
            {
                type = CSharpTokenTypes.IDENTIFIER;
            }

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber, isDeclaration);
            return tokenModel;
        }

        /// <summary>
        /// Given a syntax token identifier that represents a symbol's usage
        /// generate and return the proper HTML for this symbol
        /// </summary>
        public Token ProcessSymbolUsage(SyntaxToken token, ISymbol symbol)
        {
            string fullName = symbol.ToString();
            string value = token.ToString();
            string type = String.Empty;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            if (symbol is INamedTypeSymbol)
            {
                type = CSharpTokenTypes.TYPE;
            }
            else
            {
                type = CSharpTokenTypes.IDENTIFIER;
            }
            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            
            //If we can find the declaration, we'll link it ourselves
            if (symbol.DeclaringSyntaxReferences.Any()
                && !(symbol is INamespaceSymbol))
            {
                var link = new SymbolLink(referencedSymbolName: symbol.ToString());
                tokenModel = tokenModel.WithLink(link);
            }
            //Otherwise, we try to link to the .Net Reference source
            else if (_refsourceLinkProvider.Assemblies.Contains(symbol.ContainingAssembly?.Identity?.Name)
                && !(symbol is INamespaceSymbol))
            {
                var link = new UrlLink(url: _refsourceLinkProvider.GetLink(symbol));
                tokenModel = tokenModel.WithLink(link);
            }


            return tokenModel;
        }

        public Token ProcessIdentifier(SyntaxToken token)
        {
            //Check if this token is part of a declaration
            var parentSymbol = _model.GetDeclaredSymbol(token.Parent);
            if (parentSymbol != null)
                return ProcessDeclarationToken(token, parentSymbol);

            //Find the symbol this token references
            var symbolInfo = _model.GetSymbolInfo(token.Parent);
            if (symbolInfo.Symbol != null)
                return ProcessSymbolUsage(token, symbolInfo.Symbol);

            //Otherwise it references something we don't
            //have semantic information on...
            return ProcessOtherToken(token);
        }
    }
}
