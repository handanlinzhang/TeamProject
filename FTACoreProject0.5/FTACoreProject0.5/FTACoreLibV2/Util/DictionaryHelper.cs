using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FTACoreLibV2.Util
{
    class DictionaryHelper
    {
        public static bool IsSameDictionary<T>(Dictionary<string, T> a, Dictionary<string, T> b)
        {
            return a.Keys.Aggregate((x, y) => x + y) == a.Keys.Aggregate((x, y) => x + y);
        }
    }
}
