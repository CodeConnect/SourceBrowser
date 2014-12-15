using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Search.DocumentFields
{
    /// <summary>
    /// A class containing the names of the document fields within our Lucene Index
    /// </summary>
    public static class TokenFields
    {
        public const string Id = "Id";
        public const string Path = "Path";
        public const string FullName = "FullName";
        public const string Name = "Name";
        public const string Username = "Username";
        public const string Repository = "Repository";
        public const string LineNumber = "LineNumber";
    }
}
