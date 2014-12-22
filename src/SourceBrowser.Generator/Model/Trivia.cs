using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public class Trivia
    {
        public string Type { get; }
        public string Value { get; }
        public Trivia(string value, string type)
        {
            Value = value;
            Type = type;
        }
    }
}
