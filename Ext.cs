using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuLigaViewer;

public static class Ext
{
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
