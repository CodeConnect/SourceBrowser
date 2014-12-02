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

            var metadataSavePath = documentSavePath + ".json";
            createAndSaveMetadata(documentModel, metadataSavePath);

            //TODO: Write the HTML to the appropriate path

            using (var sw = new StreamWriter(documentSavePath))
            {
                var tokenTypes = documentModel.Tokens.Select(n => n.Type).Distinct();

                foreach (var token in documentModel.Tokens)
                {
                    processToken(sw, token);
                }
            }

            base.VisitDocument(documentModel);
        }

        private void createAndSaveMetadata(DocumentModel documentModel, string metadataPath)
        {
            var metadata = new
            {
                NumberOfLines = documentModel.NumberOfLines,
            };

            var json = JObject.FromObject(metadata);

            using (var sw = new StreamWriter(metadataPath))
            {
                sw.Write(json);
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
                    processSymbolLink(sw, token);

                var urlLink = token.Link as UrlLink;
                if (urlLink != null)
                    processUrlLink(sw, token);

                sw.Write("'>");
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
                sw.Write(trivia.Value);
            }
        }
    }
}
