using NpgsqlTypes;

namespace Postgredebaser.Mapping;

/// <summary>
/// Contains information about how a property is mapped to a PostgreSQL column
/// </summary>
public class ColumnInfo
{
    public NpgsqlDbType NpgsqlDbType { get; }
    public int? Size { get; }
    public int? AdditionalSize { get; }
    public Func<object, object> CustomToDatabase { get; }
    public Func<object, object> CustomFromDatabase { get; }

    public ColumnInfo(NpgsqlDbType npgsqlDbType, int? size = null, int? additionalSize = null, Func<object, object> customToDatabase = null, Func<object, object> customFromDatabase = null)
    {
        NpgsqlDbType = npgsqlDbType;
        Size = size;
        AdditionalSize = additionalSize;
        CustomToDatabase = customToDatabase;
        CustomFromDatabase = customFromDatabase;
    }

    public string GetColumnDefinition()
    {
        return NpgsqlDbType switch
        {
            NpgsqlDbType.Integer => "INTEGER",
            NpgsqlDbType.Bigint => "BIGINT",
            NpgsqlDbType.Text => "TEXT",
            NpgsqlDbType.Varchar => Size.HasValue ? $"VARCHAR({Size})" : "VARCHAR",
            NpgsqlDbType.Boolean => "BOOLEAN",
            NpgsqlDbType.Timestamp => "TIMESTAMP",
            NpgsqlDbType.TimestampTz => "TIMESTAMPTZ",
            NpgsqlDbType.Uuid => "UUID",
            NpgsqlDbType.Numeric => AdditionalSize.HasValue ? $"NUMERIC({Size},{AdditionalSize})" : Size.HasValue ? $"NUMERIC({Size})" : "NUMERIC",
            NpgsqlDbType.Double => "DOUBLE PRECISION",
            NpgsqlDbType.Real => "REAL",
            _ => NpgsqlDbType.ToString().ToUpperInvariant()
        };
    }
}