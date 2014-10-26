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

        public string Link
        {
            //TODO:
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
