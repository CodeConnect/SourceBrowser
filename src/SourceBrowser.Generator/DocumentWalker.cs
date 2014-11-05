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

namespace SourceBrowser.Generator
{
    /// <summary>
    /// 
    /// </summary>
    public class DocumentWalker : CSharpSyntaxWalker
    {
        private SemanticModel _model;
        private ReferencesourceLinkProvider _refsourceLinkProvider;
        public DocumentModel DocumentModel { get; private set; }
        public string FilePath { get; set; }

        public DocumentWalker(IProjectItem parent, Document document, ReferencesourceLinkProvider refSourceLinkProvider): base(SyntaxWalkerDepth.Trivia)
        {
            _model = document.GetSemanticModelAsync().Result;
            _refsourceLinkProvider = refSourceLinkProvider;
            DocumentModel = new DocumentModel(parent);
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
            else
            {
                //This covers all semantically useless tokens such as punctuation
                tokenModel = ProcessOtherToken(token);
            }

            //Add trivia to the token
            tokenModel.LeadingTrivia = ProcessTrivia(token.LeadingTrivia);
            tokenModel.TrailingTrivia = ProcessTrivia(token.TrailingTrivia);

            DocumentModel.Tokens.Add(tokenModel);
        }

        private ICollection<Trivia> ProcessTrivia(SyntaxTriviaList triviaList)
        {
            var triviaModelList = triviaList.Select(n => new Trivia()
            {
                Type = n.CSharpKind().ToString(),
                Value = n.ToFullString()
            }).ToList();

            return triviaModelList;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for non-keywords and non-identifiers.
        /// </summary>
        private Token ProcessOtherToken(SyntaxToken token)
        {
            var tokenModel = new Token();
            tokenModel.FullName = token.CSharpKind().ToString();
            tokenModel.Value = token.ToString();
            tokenModel.Type = "Other";
            return tokenModel;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for a Keyword.
        /// </summary>
        public Token ProcessKeyword(SyntaxToken token)
        {
            var tokenModel = new Token();
            tokenModel.FullName = token.CSharpKind().ToString();
            tokenModel.Value = token.ToString();
            tokenModel.Type = "Keyword";
            return tokenModel;
        }

        /// <summary>
        /// Given a syntax token identifier that represents a declaration,
        /// generate and return the proper HTML for this symbol.
        /// </summary>
        public Token ProcessDeclarationToken(SyntaxToken token, ISymbol parentSymbol)
        {
            var tokenModel = new Token();
            string html = String.Empty;
            if (parentSymbol != null && parentSymbol is INamedTypeSymbol)
            {
                //This is a type declaration. We'll assume it doesn't really 
                //link to anything for now.
                tokenModel.FullName = parentSymbol.ToString();
                tokenModel.Type = token.CSharpKind().ToString();
                tokenModel.Value = token.ToString();
            }
            else
            {
                //This isn't a type declaration
                tokenModel.Value = token.ToString();
            }
            return tokenModel;
        }

        /// <summary>
        /// Given a syntax token identifier that represents a symbol's usage
        /// generate and return the proper HTML for this symbol
        /// </summary>
        public Token ProcessSymbolUsage(SyntaxToken token, ISymbol symbol)
        {
            var tokenModel = new Token();
            tokenModel.FullName = symbol.ToString();
            tokenModel.Value = token.ToString();
            tokenModel.Type = token.CSharpContextualKind().ToString();
            
            //If we can find the declaration, we'll link it ourselves
            if (symbol.DeclaringSyntaxReferences.Any())
            {
                var localLink = new SymbolLink();
                localLink.ReferencedSymbolName = symbol.ToString();
                tokenModel.Links.Add(localLink);
            }
            //Otherwise, we try to link to the .Net Reference source
            else if (_refsourceLinkProvider.Assemblies.Contains(symbol.ContainingAssembly.Identity.Name))
            {
                var referenceLink = new UrlLink();
                referenceLink.Url = _refsourceLinkProvider.GetLink(symbol);
                tokenModel.Links.Add(referenceLink);
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
