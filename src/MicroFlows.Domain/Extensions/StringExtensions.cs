using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public static class StringExtensions
{
    public static string CompactMiddle(this string t, int max)
    {
        if (t == null)
        {
            return null;
        }

        if (t.Length > max)
        {
            return t.Substring(0, max / 2 - 1) + ".." + t.Substring(t.Length - max / 2 + 1, max / 2 - 1);
        }

        return t;
    }
}
