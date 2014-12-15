using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Search.ViewModels
{
    public struct TokenViewModel
    {
        public string Id { get; } 

        public string Path { get; } 
        public string Username { get; }
        public string Repository { get; }
        public string FullyQualifiedName { get; }
        public string DisplayName
        {
            get
            {
                if (FullyQualifiedName == null)
                    return null;
                string currentName = FullyQualifiedName;
                var parensIndex = FullyQualifiedName.IndexOf("(");
                if(parensIndex != -1)
                {
                    currentName = currentName.Remove(parensIndex, currentName.Length - parensIndex);
                }

                var splitName = currentName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                return splitName.Last();
            }
        }
        public int LineNumber { get; }

        public TokenViewModel(string username, string repository, string path, string fullName, int lineNumber)
        {
            Id = username + "/" + repository + "/" + fullName;
            Username = username;
            Repository = repository;
            Path = path;
            FullyQualifiedName = fullName;
            LineNumber = lineNumber;
        }
    }
}
