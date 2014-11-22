using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Token
    {
        public string FullName { get; set; }

        public string Type { get; set; }

        public int LineNumber { get; set; }

        public ICollection<Trivia> LeadingTrivia { get; set; }

        public string Value { get; set; }

        public ICollection<Trivia> TrailingTrivia { get; set; }

        public ILink Link { get; set; }

        public bool IsDeclaration { get; set; }

        public Token()
        {
        }
    }
}
