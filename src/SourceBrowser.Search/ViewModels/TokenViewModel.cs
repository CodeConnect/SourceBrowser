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

        public string Username { get; }
        public string Repository { get; }
        public string FullName { get; }
        public int LineNumber { get; }

        public TokenViewModel(string username, string repository, string documentId, string fullName, int lineNumber)
        {
            Username = username;
            Repository = repository;
            DocumentId = documentId;
            FullName = fullName;
            LineNumber = lineNumber;
        }
    }
}
