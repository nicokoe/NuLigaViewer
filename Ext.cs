using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuLigaViewer;

public static class Ext
{
    /// <summary> Erstellt ein Dictionary aus einer Zeichenkette. </summary>
    /// <param name="s">Zeichenkette</param>
    /// <param name="pairseperator">Paarseperator</param>
    /// <param name="keyvalueseperator">Wertpaarseperator</param>
    /// <returns>Dictionary Objekt</returns>
    public static Dictionary<string, string> ToDictionary(this string s, string pairseperator, string keyvalueseperator, 
        IEqualityComparer<string>? eqComparer = null)
    {
        var d = eqComparer == null ? new Dictionary<string, string>() : new Dictionary<string, string>(eqComparer);
        if (!string.IsNullOrEmpty(s))
        {
            string[] keyValuePairs = s.Split(pairseperator.ToCharArray());
            foreach (string keyValue in keyValuePairs)
            {
                if (string.IsNullOrEmpty(keyValue))
                    continue;
                string[] kv = keyValue.Split(keyvalueseperator.ToCharArray(), 2);
                if (kv.Length == 2)
                    d[kv[0]] = kv[1];
            }
        }
        return d;
    }
}
