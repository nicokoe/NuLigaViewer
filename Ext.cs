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

    /// <summary> Verkürzt einen String auf maxLength, falls nötig. Falls ellipsis != null, hänge ellipsis an.</summary>
    public static string Truncate(this string value, int maxLength, string ellipsis)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (ellipsis == null)
            ellipsis = "";
        if (maxLength < ellipsis.Length)
            throw new ArgumentOutOfRangeException();
        return value.Length <= maxLength ? value :
            value.Substring(0, maxLength - ellipsis.Length) + ellipsis;
    }
}
