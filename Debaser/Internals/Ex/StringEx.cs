using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ObjectCreationAsStatement

namespace Debaser.Internals.Ex;

static class StringEx
{
    public static string TrimEmptyLines(this string str)
    {
        var lines = str.Split([Environment.NewLine], StringSplitOptions.None);
        var linesWithoutEmptyLeadingLines = lines.SkipWhile(string.IsNullOrWhiteSpace).ToList();
        var linesWithoutEmptyLines = Enumerable.Reverse(linesWithoutEmptyLeadingLines).SkipWhile(string.IsNullOrWhiteSpace).Reverse().ToList();
        return string.Join(Environment.NewLine, linesWithoutEmptyLines);
    }

    public static IEnumerable<string> Indented(this IEnumerable<string> lines, int indentation)
    {
        return lines.Select(str => string.Concat(new string(' ', indentation), str));
    }

    public static string TrimUntil(this string str, char c)
    {
        var index = str.IndexOf(c);

        return index < 0 ? str : str.Substring(index + 1);
    }

    public static string TrimAfter(this string str, char c)
    {
        var index = str.IndexOf(c);

        return index < 0 ? str : str.Substring(0, index);
    }

    public static bool IsValidUrl(this string str)
    {
        try
        {
            new Uri(str);
            return true;
        }
        catch
        {
            return false;
        }
    }
}