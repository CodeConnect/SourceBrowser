using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SourceBrowser.Generator
{
    public class ReferencesourceLinkProvider
    {
        private const string baseUrl = "http://referencesource.microsoft.com";
        private string assembliesUrl = baseUrl + "/assemblies.txt";

        private List<string> assemblies = new List<string>(); 

        public IEnumerable<string> Assemblies
        {
            get { return assemblies; }
        }

        public ReferencesourceLinkProvider()
        {
            
        }

        public async  Task Init()
        {
            var getReferenceAssemblies = new HttpClient();
            var referenceAssemblies = await getReferenceAssemblies.GetStringAsync(assembliesUrl);
            foreach(var line in referenceAssemblies.Split('\n'))
            {
                var parts = line.Split(';');
                if (parts.Length > 0)
                {
                    assemblies.Add(parts[0]);
                }
            }

        }

        public string GetLink(ISymbol symbol)
        {
            return baseUrl + "/" + symbol.ContainingAssembly.Name + "/a.html#" + Utilities.GetHash(GetDocumentationCommentId(symbol));

        }
        private static string GetDocumentationCommentId(ISymbol symbol)
        {
            string result = null;
            if (!symbol.IsDefinition)
            {
                symbol = symbol.OriginalDefinition;
            }

            result = symbol.GetDocumentationCommentId();

            result = result.Replace("#ctor", "ctor");

            return result;
        }


		
    }
}
