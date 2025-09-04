
// This class has been moved to Debaser.Core.Internals.Ex
// This file provides backward compatibility

// ReSharper disable ObjectCreationAsStatement

using Debaser.Core.Internals.Ex;

namespace Debaser.Internals.Ex;

static class StringEx
{
    public static string TrimEmptyLines(this string str) => Core.Internals.Ex.StringEx.TrimEmptyLines(str);
    public static IEnumerable<string> Indented(this IEnumerable<string> lines, int indentation) => Core.Internals.Ex.StringEx.Indented(lines, indentation);
}