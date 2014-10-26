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
        public ISymbol ReferencedSymbol { get; set; }

        public string GetLink()
        {
            //TODO:
            throw new NotImplementedException();
        }
    }
}
