using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SourceBrowser.Generator.Model
{
    public class SymbolLink : ILink
    {
        public string ReferencedSymbolName { get; }

        public string Link
        {
            //TODO:
            get
            {
                throw new NotImplementedException();
            }
        }

        public SymbolLink(string referencedSymbolName)
        {
            ReferencedSymbolName = referencedSymbolName;
        }
    }
}
