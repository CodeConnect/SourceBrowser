using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Token
    {
        public string FullName { get; }

        public string Type { get; }

        public int LineNumber { get; }

        public ICollection<Trivia> LeadingTrivia { get; }

        public string Value { get; }

        public ICollection<Trivia> TrailingTrivia { get; }

        public ILink Link { get; }

        public bool IsDeclaration { get; }

        public bool IsSearchable { get; }

        public DocumentModel Document { get; }

        public Token(DocumentModel document, string fullName, string value, string type, int lineNumber, bool isDeclaration = false, bool isSearchable = false)
        {
            Document = document;
            FullName = fullName;
            Value = value;
            Type = type;
            LineNumber = lineNumber;
            IsDeclaration = isDeclaration;
            IsSearchable = isSearchable;
        }

        private Token(Token oldToken, ILink link) :
            this(oldToken.Document, oldToken.FullName, oldToken.Value, oldToken.Type, oldToken.LineNumber, oldToken.IsDeclaration, oldToken.IsSearchable)
        {
            Link = link;
            LeadingTrivia = oldToken.LeadingTrivia;
            TrailingTrivia = oldToken.TrailingTrivia;
        }

        private Token(Token oldToken, ICollection<Trivia> leading, ICollection<Trivia> trailing) :
            this(oldToken.Document, oldToken.FullName, oldToken.Value, oldToken.Type, oldToken.LineNumber, oldToken.IsDeclaration, oldToken.IsSearchable)
        {
            Link = oldToken.Link;
            LeadingTrivia = leading;
            TrailingTrivia = trailing;
        }



        /// <summary>
        /// Create a new token with the provided link.
        /// </summary>
        /// <returns>A new token containing the provided link.</returns>
        public Token WithLink(ILink link)
        {
            var newToken = new Token(this, link);
            return newToken;
        }

        /// <summary>
        /// Creates a new token with the provided trivia.
        /// </summary>
        /// <returns>A new token containing the provided trivia.</returns>
        public Token WithTrivia(ICollection<Trivia> leading, ICollection<Trivia> trailing)
        {
            var newToken = new Token(this, leading, trailing);
            return newToken;
        }
    }
}
