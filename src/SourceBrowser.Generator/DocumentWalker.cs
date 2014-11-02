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
        private StringBuilder _stringBuilder = new StringBuilder();
        private int _numLines = 0;
        public string FilePath { get; set; }

        private DocumentInfo DocInfo;
        public DocumentInfo GetDocumentInfo()
        {
            DocInfo.HtmlContent = _stringBuilder.ToString();
            return DocInfo;
        }

        public DocumentWalker(SemanticModel model, Document document, ReferencesourceLinkProvider refSourceLinkProvider, Dictionary<string, string> typeLookup) : base(SyntaxWalkerDepth.Trivia)
        {
            _model = document.GetSemanticModelAsync().Result;
            _refsourceLinkProvider = refSourceLinkProvider;
            FilePath = document.GetRelativeFilePath();
            DocInfo.FileName = FilePath;
            DocInfo.NumberOfLines = document.GetTextAsync().Result.Lines.Count;
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
                str = ProcessIdentifier(token);
            }
            else
            {
                //This covers all semantically useless tokens such as punctuation
                tokenModel = ProcessOtherToken(token);
            }

            //Add trivia to the token
            tokenModel.LeadingTrivia = ProcessTrivia(token.LeadingTrivia);
            tokenModel.TrailingTrivia = ProcessTrivia(token.TrailingTrivia);
        }

        private IEnumerable<Trivia> ProcessTrivia(SyntaxTriviaList triviaList)
        {
            var triviaModelList = triviaList.Select(n => new Trivia()
            {
                Type = n.CSharpKind().ToString(),
                Value = n.ToFullString()
            });
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
        public string ProcessDeclaration(SyntaxToken token, ISymbol parentSymbol)
        {
            string html = String.Empty;
            if (parentSymbol != null && parentSymbol is INamedTypeSymbol)
            {
                //Type Declaration
                html = "<span style='color:#2B91AF'>" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
                return html;
            }
            //This isn't a type declaration
            return HttpUtility.HtmlEncode(token.ToString());
        }

        public string ProcessTypeUsage(SyntaxToken token, ISymbol symbol)
        {
            //Type usage
            string fullName = symbol.ToString();
            string html = String.Empty;
            string referencedURL = "";
            //Check if we can link to this type
            //if (_typeLookup.TryGetValue(fullName, out referencedURL))
            {
                var relativePath = Utilities.MakeRelativePath(this.FilePath, referencedURL);

                html = "<span style='color:#2B91AF'>";
                html += "<a href=";
                html += relativePath;
                html += ">";
                html += HttpUtility.HtmlEncode(token.ToString());
                html += "</a>";
                html += "</span>";
            }
            //else if (refsourceLinkProvider.Assemblies.Contains(symbol.ContainingAssembly.Identity.Name))
            {
                html = "<span>";
                html += "<a style='color:black' href=";
                html += _refsourceLinkProvider.GetLink(symbol);
                html += ">";
                html += HttpUtility.HtmlEncode(token.ToString());
                html += "</a>";
                html += "</span>";
            }
            //else
            {
                //otherwise, just color it 
                html += "<span style='color:#2B91AF'>" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
            }

            return html;
        }

        public string ProcessMemberUsage(SyntaxToken token, ISymbol symbol)
        {
            string fullName = symbol.ToString();
            string html = string.Empty;
            //Check to see if we can link to this method
            //if (_typeLookup.TryGetValue(fullName, out referencedURL))
            {
                html += HttpUtility.HtmlEncode(token.ToString());
            }
            //else if (refsourceLinkProvider.Assemblies.Contains(symbol.ContainingAssembly.Identity.Name))
            {
                html += _refsourceLinkProvider.GetLink(symbol);
                html += HttpUtility.HtmlEncode(token.ToString());
            }
            //else
            {
                //otherwise, just color it 
                html += "<span>" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
            }

            return html;
        }


        /// <summary>
        /// Given a syntax token identifier that represents a symbol's usage
        /// generate and return the proper HTML for this symbol
        /// </summary>
        public string ProcessSymbolUsage(SyntaxToken token, ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
            {
                return ProcessTypeUsage(token, symbol);
            }

            if (symbol is IMethodSymbol)
            {
                return ProcessMemberUsage(token, symbol);
            }

            if (symbol is IPropertySymbol)
            {
                return ProcessMemberUsage(token, symbol);
            }

            return HttpUtility.HtmlEncode(token.ToString());
        }

        public string ProcessIdentifier(SyntaxToken token)
        {
            //Check if this token is part of a declaration
            var parentSymbol = _model.GetDeclaredSymbol(token.Parent);

            if (parentSymbol != null)
                return ProcessDeclaration(token, parentSymbol);

            //Find the symbol this token references
            var symbolInfo = _model.GetSymbolInfo(token.Parent);

            if (symbolInfo.Symbol != null)
            {
                return ProcessSymbolUsage(token, symbolInfo.Symbol);
            }

            return HttpUtility.HtmlEncode(token.ToString());
        }
    }
}
