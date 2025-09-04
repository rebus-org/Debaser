using System.Reflection;
using FastMember;
using Npgsql;

namespace Postgredebaser.Mapping;

/// <summary>
/// Represents a single mapped property for PostgreSQL
/// </summary>
public class ClassMapProperty
{
    readonly Func<object, object> _toDatabase;
    readonly Func<object, object> _fromDatabase;

    /// <summary>
    /// Gets the name of the database column
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets the name of the property
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the database column information for this property
    /// </summary>
    public ColumnInfo ColumnInfo { get; }

    /// <summary>
    /// Gets whether this property is part of the primary key
    /// </summary>
    public bool IsKey { get; }

    internal ClassMapProperty(string propertyName, string columnName, ColumnInfo columnInfo, bool isKey, Func<object, object> toDatabase, Func<object, object> fromDatabase)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        ColumnInfo = columnInfo ?? throw new ArgumentNullException(nameof(columnInfo));
        IsKey = isKey;
        _toDatabase = toDatabase ?? throw new ArgumentNullException(nameof(toDatabase));
        _fromDatabase = fromDatabase ?? throw new ArgumentNullException(nameof(fromDatabase));
    }

    public object ToDatabase(object value) => _toDatabase(value);

    public object FromDatabase(object value) => _fromDatabase(value);

    public async Task WriteToAsync(NpgsqlBinaryImporter writer, object row)
    {
        var accessor = TypeAccessor.Create(row.GetType());
        var value = accessor[row, PropertyName];
        var dbValue = ToDatabase(value);

        if (dbValue == null || dbValue == DBNull.Value)
        {
            await writer.WriteNullAsync();
        }
        else
        {
            await writer.WriteAsync(dbValue, ColumnInfo.NpgsqlDbType);
        }
    }

    public override string ToString()
    {
        return $"{PropertyName} -> {ColumnName} ({ColumnInfo.NpgsqlDbType})";
    }
}