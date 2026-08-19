using System.Text.Json;

namespace Postgredebaser.Internals.Naming;

/// <summary>
/// Translates CLR type and property names into PostgreSQL identifiers
/// </summary>
static class NameConverter
{
    /// <summary>
    /// Converts the given <paramref name="name"/> to snake case, following PostgreSQL naming conventions.
    /// Turns e.g. <code>OrderLine</code> into <code>order_line</code>, <code>HTTPStatus</code> into
    /// <code>http_status</code>, and <code>Sha256Hash</code> into <code>sha256_hash</code>.
    /// </summary>
    public static string ToPostgresName(string name) => JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
}
