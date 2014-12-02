using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using SourceBrowser.Generator.Extensions;

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

        public void Init()
        {
            var getReferenceAssemblies = new HttpClient();
            var referenceAssemblies = getReferenceAssemblies.GetStringAsync(assembliesUrl).Result;
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
            var symbolId = symbol.GetSymbolId();
            if (symbolId != null)
            {
                return baseUrl + "/" + symbol.ContainingAssembly.Name + "/a.html#" + Utilities.GetHash(symbolId);
            }
            else
            {
                return null;
            }
        }
    }
}
