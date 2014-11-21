using SourceBrowser.Generator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML
    /// </summary>
    public class HtmlTransformer : AbstractWorkspaceVisitor
    {
        private WorkspaceModel _root;
        private string _savePath;
        public HtmlTransformer(WorkspaceModel root, string savePath)
        {
            _root = root;
            _savePath = savePath;
        }

        protected override void VisitFolder(FolderModel folderModel)
        {
            base.VisitFolder(folderModel);
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            var documentSavePath = Path.Combine(_savePath, documentModel.RelativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(documentSavePath));
            //TODO: Write the HTML to the appropriate path

            using (var sw = new StreamWriter(documentSavePath))
            {
                var tokenTypes = documentModel.Tokens.Select(n => n.Type).Distinct();
                var leadingTriviaTypes = documentModel.Tokens.SelectMany(n => n.LeadingTrivia.Select(m => m.Type)).Distinct();
                var trailingTriviaTypes = documentModel.Tokens.SelectMany(n => n.TrailingTrivia.Select(m => m.Type)).Distinct();

                foreach (var token in documentModel.Tokens)
                {
                    processToken(sw, token);
                }
            }

            base.VisitDocument(documentModel);
        }

        private void processToken(StreamWriter sb, Token token)
        {
            processTriviaCollection(sb, token.LeadingTrivia);

            switch (token.Type)
            {
                case "Keyword":
                    processKeyword(sb, token);
                    break;
                case "IdentifierToken":
                    processIdentifier(sb, token);
                    break;
                case "Other":
                    processOther(sb, token);
                    break;
                default:
                    throw new InvalidOperationException("Invalid token type");
            }

            processTriviaCollection(sb, token.TrailingTrivia);
        }

        private void processIdentifier(StreamWriter sw, Token token)
        {
            sw.Write(token.Value);
        }

        private void processOther(StreamWriter sw, Token token)
        {
            sw.Write(token.Value);
        }

        private void processKeyword(StreamWriter sw, Token token)
        {
            sw.Write("<span class='keyword'>");
            sw.Write(token.Value);
            sw.Write("</span>");
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
                sw.Write("<span class='comment'>");
                sw.Write(trivia.Value);
                sw.Write("</span>");
            }
            else
            {
                sw.Write(trivia.Value);
            }
        }
    }
}
