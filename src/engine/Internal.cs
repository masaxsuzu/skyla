using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Skyla.Tests")]

static class DictionaryExtension
{
    public static void Put<Key, Value>(this Dictionary<Key, Value> map, Key key, Value value) where Key : notnull
    {
        if (map.ContainsKey(key))
        {
            map[key] = value;
        }
        else
        {
            map.Add(key, value);
        }
    }
}
