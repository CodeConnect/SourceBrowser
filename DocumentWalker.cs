using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceBrowser.Generator
{
    /// <summary>
    /// 
    /// </summary>
    public class DocumentWalker : CSharpSyntaxWalker
    {
        string _html = String.Empty;
        Dictionary<string, string> _typeLookup;
        private SemanticModel _model;
        private StringBuilder _stringBuilder = new StringBuilder();
        private int _numLines = 0;
        public string FilePath { get; set; }

        private DocumentInfo DocInfo;
        public DocumentInfo GetDocumentInfo()
        {
            DocInfo.HtmlContent = _stringBuilder.ToString();
            return DocInfo;
        }

        public DocumentWalker(SemanticModel model, Document document, Dictionary<string, string> typeLookup) : base(SyntaxWalkerDepth.Trivia)
        {
            _model = model;
            _typeLookup = typeLookup;
            FilePath = document.GetRelativeFilePath();

            DocInfo.FileName = FilePath;
            DocInfo.NumberOfLines = document.GetTextAsync().Result.Lines.Count;
        }

        public override void VisitLeadingTrivia(SyntaxToken token)
        {
            foreach (var tr in token.LeadingTrivia)
            {
                this.VisitTrivia(tr);
            }
        }

        public override void VisitTrailingTrivia(SyntaxToken token)
        {
            foreach (var tr in token.TrailingTrivia)
            {
                this.VisitTrivia(tr);
            }
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            string htmlTrivia = String.Empty;
            if (trivia.CSharpKind() == SyntaxKind.SingleLineCommentTrivia ||
                trivia.CSharpKind() == SyntaxKind.MultiLineCommentTrivia ||
                trivia.CSharpKind() == SyntaxKind.MultiLineDocumentationCommentTrivia ||
                trivia.CSharpKind() == SyntaxKind.SingleLineDocumentationCommentTrivia)
            {
                htmlTrivia += "<span style='color:green'>";
                htmlTrivia += HttpUtility.HtmlEncode(trivia.ToString());
                htmlTrivia += "</span>";
            }
            else if (trivia.CSharpKind() == SyntaxKind.RegionDirectiveTrivia ||
                     trivia.CSharpKind() == SyntaxKind.EndRegionDirectiveTrivia)
            {
                htmlTrivia += "<span style='color:blue'>";
                // We don't visit insides of region directives,
                // so we need to use ToFullString() to get the new line and whitespace
                htmlTrivia += HttpUtility.HtmlEncode(trivia.ToFullString());
                htmlTrivia += "</span>";
            }
            else
            {
                htmlTrivia += HttpUtility.HtmlEncode(trivia.ToString());
            }

            _stringBuilder.Append(htmlTrivia);
            base.VisitTrivia(trivia);
        }

        public override void VisitToken(SyntaxToken token)
        {
            this.VisitLeadingTrivia(token);
            string str = String.Empty;

            if (token.IsKeyword())
            {
                str = ProcessKeyword(token);
            }
            else if(token.CSharpKind() == SyntaxKind.IdentifierToken)
            {
                str = ProcessIdentifier(token);
            }
            else
            {
                str = HttpUtility.HtmlEncode(token.ToString());
            }

            _stringBuilder.Append(str);
            this.VisitTrailingTrivia(token);
        }

        public string ProcessKeyword(SyntaxToken token)
        {
            string str = "<span style='color:blue'>" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
            return str;
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
            string referencedURL;
            //Check if we can link to this type
            if (_typeLookup.TryGetValue(fullName, out referencedURL))
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
            else
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
            string referencedURL;
            //Check to see if we can link to this method
            if (_typeLookup.TryGetValue(fullName, out referencedURL))
            {
                var relativePath = Utilities.MakeRelativePath(this.FilePath, referencedURL);

                html = "<span>";
                html += "<a style='color:black' href=";
                html += relativePath;
                html += ">";
                html += HttpUtility.HtmlEncode(token.ToString());
                html += "</a>";
                html += "</span>";
            }
            else
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
