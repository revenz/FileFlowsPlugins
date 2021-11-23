using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoNodes
{
    internal static class ExtensionMethods
    {
        public static void AddOrUpdate(this Dictionary<string, object> dict, string key, object value) {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);   
        }
    }
}
