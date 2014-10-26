using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Model
{
    public interface ILink
    {
        /// <summary>
        /// Regardless of the type of link, it must be representable as a single string.
        /// </summary>
        /// <returns></returns>
        string GetLink();
    }
}
