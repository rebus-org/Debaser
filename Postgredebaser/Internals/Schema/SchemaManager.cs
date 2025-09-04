using Npgsql;
using Postgredebaser.Internals.Sql;
using Postgredebaser.Mapping;

namespace Postgredebaser.Internals.Schema;

class SchemaManager
{
    readonly List<ClassMapProperty> _mutableProperties;
    readonly List<ClassMapProperty> _keyProperties;
    readonly List<ClassMapProperty> _properties;
    readonly NpgsqlConnectionFactory _factory;
    readonly string _tableName;
    readonly string _schema;
    readonly string _extraCriteria;
    readonly string _qualifiedTableName;

    public SchemaManager(NpgsqlConnectionFactory factory, string tableName, IEnumerable<ClassMapProperty> keyProperties, IEnumerable<ClassMapProperty> properties, string schema = "public", string extraCriteria = null)
    {
        _factory = factory;
        _tableName = tableName;
        _schema = schema;
        _extraCriteria = extraCriteria;
        _keyProperties = keyProperties.ToList();
        _properties = properties.ToList();
        _mutableProperties = _properties.Except(_keyProperties).ToList();
        _qualifiedTableName = $"\"{_schema}\".\"{_tableName}\"";
    }

    public string TableName => _tableName;

    public void CreateSchema(bool createTable)
    {
        using var connection = OpenNpgsqlConnection();
        using var transaction = connection.BeginTransaction();

        var createTableScript = GetCreateSchemaScript();

        if (createTable)
        {
            ExecuteStatement(connection, transaction, createTableScript);
        }

        transaction.Commit();
    }

    public string GetCreateSchemaScript()
    {
        var columns = _properties.Select(p => $"\"{p.ColumnName}\" {p.ColumnInfo.GetColumnDefinition()}").ToList();
        
        if (_keyProperties.Any())
        {
            var keyColumns = string.Join(", ", _keyProperties.Select(p => $"\"{p.ColumnName}\""));
            columns.Add($"PRIMARY KEY ({keyColumns})");
        }

        var columnsText = string.Join(",\n    ", columns);

        return $@"CREATE TABLE IF NOT EXISTS {_qualifiedTableName} (
    {columnsText}
);";
    }

    public void DropSchema(bool dropTable)
    {
        using var connection = OpenNpgsqlConnection();
        using var transaction = connection.BeginTransaction();

        var dropTableScript = GetDropSchemaScript();

        if (dropTable)
        {
            ExecuteStatement(connection, transaction, dropTableScript);
        }

        transaction.Commit();
    }

    public string GetDropSchemaScript()
    {
        return $"DROP TABLE IF EXISTS {_qualifiedTableName};";
    }

    public string GetQuery(string criteria = null)
    {
        var sql = $"SELECT {string.Join(", ", _properties.Select(p => $"\"{p.ColumnName}\""))} FROM {_qualifiedTableName}";
        
        if (!string.IsNullOrWhiteSpace(criteria))
        {
            sql += $" WHERE {criteria}";
        }
        
        return sql;
    }

    public string GetUpsertSql()
    {
        // For now, return a COPY command - we'll implement proper upsert logic later
        var columns = string.Join(", ", _properties.Select(p => $"\"{p.ColumnName}\""));
        return $"COPY {_qualifiedTableName} ({columns}) FROM STDIN (FORMAT BINARY)";
    }

    NpgsqlConnection OpenNpgsqlConnection()
    {
        return _factory.OpenNpgsqlConnection();
    }

    static void ExecuteStatement(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}