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
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.FindSymbols;

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

        private Document _document;

        public CSWalker(IProjectItem parent, Document document, ReferencesourceLinkProvider refSourceLinkProvider) : base(SyntaxWalkerDepth.Trivia)
        {
            _model = document.GetSemanticModelAsync().Result;
            _refsourceLinkProvider = refSourceLinkProvider;
            string containingPath = document.GetRelativeFilePath();

            var numberOfLines = document.GetTextAsync().Result.Lines.Count + 1;
            DocumentModel = new DocumentModel(parent, document.Name, numberOfLines);
            FilePath = document.GetRelativeFilePath();
            _refsourceLinkProvider = refSourceLinkProvider;
            _document = document;
        }

        public override void VisitToken(SyntaxToken token)
        {
            Token tokenModel = null;

            if (token.IsKeyword())
            {
                tokenModel = ProcessKeyword(token);
            }
            else if (token.CSharpKind() == SyntaxKind.IdentifierToken)
            {
                tokenModel = ProcessIdentifier(token);
            }
            else if (token.CSharpKind() == SyntaxKind.StringLiteralToken)
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

        public Token ProcessSymbolUsage(SyntaxToken token, ISymbol symbol, bool isDeclaration)
        {
            string fullName = GetSymbolName(symbol);
            string value = token.ToString();
            string type = String.Empty;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            bool isSearchable = isDeclaration;

            if (symbol is INamedTypeSymbol)
            {
                type = CSharpTokenTypes.TYPE;
            }
            else
            {
                type = CSharpTokenTypes.IDENTIFIER;
            }

            //Do not allow us to search locals
            if (symbol.Kind == SymbolKind.Local || symbol.Kind == SymbolKind.Parameter)
            {
                isSearchable = false;
            }

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber, isDeclaration, isSearchable);

            //If we can find the declaration, we'll link it ourselves
            if (symbol.DeclaringSyntaxReferences.Any()
                && !(symbol is INamespaceSymbol))
            {
                var link = new SymbolLink(referencedSymbolName: fullName);
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

        private string GetSymbolName(ISymbol symbol)
        {
            if (symbol.Kind == SymbolKind.Parameter || symbol.Kind == SymbolKind.Local)
            {
                var containingName = symbol.ContainingSymbol.ToString();
                var name = containingName + "::" + symbol.Name;
                return name;
            }
            else
            {
                return symbol.ToString();
            }
        }

        public Token ProcessIdentifier(SyntaxToken token)
        {
            //Check if this token is part of a declaration
            bool isDeclaration = false;
            if (_model.GetDeclaredSymbol(token.Parent) != null)
            {
                isDeclaration = true;
            }
            var startPosition = token.GetLocation().SourceSpan.Start;
            //Note: We're using the SymbolFinder as it correctly resolves 
            var symbol = SymbolFinder.FindSymbolAtPosition(_model, startPosition, _document.Project.Solution.Workspace);
            if (symbol != null)
            {
                return ProcessSymbolUsage(token, symbol, isDeclaration);
            }

            //Otherwise it references something we don't
            //have semantic information on...
            return ProcessOtherToken(token);
        }
    }
}
