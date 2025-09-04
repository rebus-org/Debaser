// ReSharper disable ObjectCreationAsStatement

namespace Debaser.Core.Internals.Ex;

public static class StringEx
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
}