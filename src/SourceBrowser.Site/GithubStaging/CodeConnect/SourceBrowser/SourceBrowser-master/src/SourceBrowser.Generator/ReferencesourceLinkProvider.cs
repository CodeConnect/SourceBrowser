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
            return baseUrl + "/" + symbol.ContainingAssembly.Name + "/a.html#" + GetHash(GetDocumentationCommentId(symbol));

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


		public static string GetHash(string result) {
			result = GetMD5Hash(result, 8);
			return result;
		}
		static string GetMD5Hash(string input, int digits) {
			using (var md5 = MD5.Create()) {
				var bytes = Encoding.UTF8.GetBytes(input);
				var hashBytes = md5.ComputeHash(bytes);
				return ByteArrayToHexString(hashBytes, digits);
			}
		}
		static string ByteArrayToHexString(byte[] bytes, int digits = 0) {
			if (digits == 0) {
				digits = bytes.Length * 2;
			}

			char[] c = new char[digits];
			byte b;
			for (int i = 0; i < digits / 2; i++) {
				b = ((byte)(bytes[i] >> 4));
				c[i * 2] = (char)(b > 9 ? b + 87 : b + 0x30);
				b = ((byte)(bytes[i] & 0xF));
				c[i * 2 + 1] = (char)(b > 9 ? b + 87 : b + 0x30);
			}

			return new string(c);
		}
    }
}
