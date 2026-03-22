using System;
using System.Text;
using LongArithmetic.Core.Models;

namespace LongArithmetic.Core.Helpers;

public static class Helpers
{
    public static string Trim(string s)
    {
        var trimmed = s.TrimStart('0');
        return trimmed == string.Empty ? "0" : trimmed;
    }

    public static int Compare(BigNumber a, BigNumber b)
    {
        a = Trim(a);
        b = Trim(b);

        return a.Value.Length != b.Value.Length
            ? a.Value.Length.CompareTo(b.Value.Length)
            : string.Compare(a, b, StringComparison.Ordinal);
    }

    public static string DivideByTwo(string number)
    {
        var sb = new StringBuilder();
        var carry = 0;

        foreach (var c in number)
        {
            var current = carry * 10 + (c - '0');
            sb.Append(current / 2);
            carry = current % 2;
        }

        return Trim(sb.ToString());
    }

    public static string MultiplyByDigit(string a, int digit)
    {
        if (digit == 0) return "0";
        if (digit == 1) return Trim(a);

        var carry = 0;
        var sb = new StringBuilder();

        for (var i = a.Length - 1; i >= 0; i--)
        {
            var prod = (a[i] - '0') * digit + carry;
            carry = prod / 10;
            sb.Insert(0, (prod % 10).ToString());
        }

        if (carry > 0)
            sb.Insert(0, carry.ToString());

        return Trim(sb.ToString());
    }

    public static void Swap(ref BigNumber a, ref BigNumber b)
    {
        (a, b) = (b, a);
    }

    public static int GetCutPosition(string first, string second)
    {
        var min = Math.Min(first.Length, second.Length);
        if (min == 1) return 1;
        if (min % 2 == 0) return min / 2;
        return min / 2 + 1;
    }

    public static string GetFirstPart(string str, int cutPos)
    {
        return str.Remove(str.Length - cutPos);
    }

    public static string GetSecondPart(string str, int cutPos)
    {
        return str.Substring(str.Length - cutPos);
    }
}
