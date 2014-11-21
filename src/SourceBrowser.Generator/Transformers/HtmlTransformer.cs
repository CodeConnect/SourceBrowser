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
            var documentSavePath = Path.Combine(_savePath, documentModel.ContainingPath, documentModel.Name);
            //TODO: Write the HTML to the appropriate path

            StringBuilder sb = new StringBuilder(documentModel.Tokens.Count);
            var tokenTypes = documentModel.Tokens.Select(n => n.Type).Distinct();
            var leadingTriviaTypes = documentModel.Tokens.SelectMany(n => n.LeadingTrivia.Select(m => m.Type)).Distinct();
            var trailingTriviaTypes = documentModel.Tokens.SelectMany(n => n.TrailingTrivia.Select(m => m.Type)).Distinct();

            foreach (var token in documentModel.Tokens)
            {
                processToken(sb, token);
            }

            base.VisitDocument(documentModel);
        }

        private void processToken(StringBuilder sb, Token token)
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

        private void processIdentifier(StringBuilder sb, Token token)
        {
            sb.Append(token.Value);
        }

        private void processOther(StringBuilder sb, Token token)
        {
            sb.Append(token.Value);
        }

        private void processKeyword(StringBuilder sb, Token token)
        {
            sb.Append(token.Value);
        }


        private void processTriviaCollection(StringBuilder sb, ICollection<Trivia> triviaCollection)
        {
            foreach(var trivia in triviaCollection)
            {
                processTrivia(sb, trivia);
            }
        }

        private void processTrivia(StringBuilder sb, Trivia trivia)
        {
            if(trivia.Type.Contains("Comment"))
            {
                sb.Append("<span class='comment'>");
                sb.Append(trivia.Value);
                sb.Append("</span>");
            }
            else
            {
                sb.Append(trivia.Value);
            }
        }
    }
}
