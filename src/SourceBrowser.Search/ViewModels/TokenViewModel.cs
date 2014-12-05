using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Search.ViewModels
{
    public struct TokenViewModel
    {
        public string DocumentId { get; }
        public string FullName { get; }
        public int LineNumber { get; }

        public TokenViewModel(string documentId, string fullName, int lineNumber)
        {
            DocumentId = documentId;
            FullName = fullName;
            LineNumber = lineNumber;
        }
    }
}
