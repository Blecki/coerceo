using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public static class UpsertExtension
    {
        /// <summary>
        /// If the dictionary contains key, update the associated value.
        /// Otherwise, insert it.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="Dict"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void Upsert<K, V>(this Dictionary<K, V> Dict, K Key, V Value)
        {
            if (Dict.ContainsKey(Key)) Dict[Key] = Value;
            else Dict.Add(Key, Value);
        }
    }
}
