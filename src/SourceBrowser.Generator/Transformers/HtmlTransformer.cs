using Newtonsoft.Json.Linq;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator.Model.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML
    /// </summary>
    public class HtmlTransformer : AbstractWorkspaceVisitor
    {
        private string _savePath;
        private Dictionary<string, Token> _tokenLookup;
        public HtmlTransformer(Dictionary<string, Token> tokenLookup, string savePath)
        {
            _tokenLookup = tokenLookup;
            _savePath = savePath;
        }
        protected override void VisitDocument(DocumentModel documentModel)
        {
            var documentSavePath = Path.Combine(_savePath, documentModel.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(documentSavePath));

            using (var sw = new StreamWriter(documentSavePath))
            {
                sw.WriteLine("<table cellpadding='0' cellspacing='0'>");
                sw.WriteLine("<tbody>");
                sw.WriteLine("<tr>");
                sw.WriteLine("<td valign='top' style='min-diwth:65px'>");
                sw.WriteLine("<pre id='line-numbers'>");

                //Create the sidebar with line numbers
                this.createLineNumberHtml(sw, documentModel.NumberOfLines);

                sw.WriteLine("</pre>"); //line-numbers
                sw.WriteLine("</td>");

                sw.WriteLine("<td valign='top'>");
                sw.WriteLine("<pre class='source-code'>");
                

                foreach (var token in documentModel.Tokens)
                {
                    processToken(sw, token);
                }

                sw.WriteLine("</pre>");
                sw.WriteLine("</td>");
                sw.WriteLine("</tr>");
                sw.WriteLine("</tbody>");
                sw.WriteLine("</table>");

            }

            base.VisitDocument(documentModel);
        }

        private void createLineNumberHtml(StreamWriter sw, int numberOfLines)
        {
            //Note that we start at line 1, as most editors dont start at line 0.
            for(int i = 1; i < numberOfLines; i++)
            {
                sw.WriteLine("<a href='#" + i + "' name='" + i + "'>" + i + "</a>");
            }
        }

        private void processToken(StreamWriter sw, Token token)
        {
            processTriviaCollection(sw, token.LeadingTrivia);

            sw.Write("<span class='");
            sw.Write(HttpUtility.HtmlEncode(token.Type));
            sw.Write("'>");
            if(token.Link != null)
            {
                sw.Write("<a href='");

                var symbolLink = token.Link as SymbolLink;
                if (symbolLink != null)
                {
                    processSymbolLink(sw, token);
                    sw.Write("'>");
                }

                var urlLink = token.Link as UrlLink;
                if (urlLink != null)
                {
                    processUrlLink(sw, token);
                    sw.Write("'");
                    sw.Write(" target='_blank'");
                    sw.Write(">");
                }

                sw.Write(HttpUtility.HtmlEncode(token.Value));
                sw.Write("</a>");
            }
            else
            {
                sw.Write(HttpUtility.HtmlEncode(token.Value));
            }

            sw.Write("</span>");
            processTriviaCollection(sw, token.TrailingTrivia);
        }

        private void processUrlLink(StreamWriter sw, Token token)
        {
            var urlLink = token.Link as UrlLink;
            var url = urlLink.Url;
            sw.Write(url);
        }

        private void processSymbolLink(StreamWriter sw, Token token)
        {
            var symbolLink = token.Link as SymbolLink;
            var name = symbolLink.ReferencedSymbolName;
            Token referencedToken;
            if(_tokenLookup.TryGetValue(name, out referencedToken))
            {
                var relPath = Utilities.MakeRelativePath(token.Document.RelativePath, referencedToken.Document.RelativePath);
                var path = relPath + "#" + referencedToken.LineNumber.ToString();
                sw.Write(path);
            }
            else
            {
                //If we can't find it, just make the link point nowhere.
                sw.Write('#');
            }
        }


        private void processTriviaCollection(StreamWriter sw, ICollection<Trivia> triviaCollection)
        {
            foreach(var trivia in triviaCollection)
            {
                processTrivia(sw, trivia);
            }
        }

        private void processTrivia(StreamWriter sw, Trivia trivia)
        {
            if(trivia.Type.Contains("Comment"))
            {
                sw.Write("<span class='Comment'>");
                sw.Write(HttpUtility.HtmlEncode(trivia.Value));
                sw.Write("</span>");
            }
            else
            {
                sw.Write(HttpUtility.HtmlEncode(trivia.Value));
            }
        }
    }
}
