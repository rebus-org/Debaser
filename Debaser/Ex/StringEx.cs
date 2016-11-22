using System.Collections.Generic;
using System.Linq;

namespace Debaser.Ex
{
    static class StringEx
    {
        public static IEnumerable<string> Indented(this IEnumerable<string> lines, int indentation)
        {
            return lines.Select(str => string.Concat(new string(' ', indentation), str));
        }    
    }
}