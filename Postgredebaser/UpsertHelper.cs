using System.Collections.Concurrent;
using System.Data;
using Debaser.Core;
using Debaser.Core.Internals.Exceptions;
using FastMember;
using Npgsql;
using Postgredebaser.Internals.Query;
using Postgredebaser.Internals.Schema;
using Postgredebaser.Internals.Sql;
using Postgredebaser.Internals.Data;
using Postgredebaser.Internals.Reflection;
using Postgredebaser.Mapping;

namespace Postgredebaser;

/// <summary>
/// PostgreSQL UpsertHelper. Create an instance and start working with your data
/// </summary>
public class UpsertHelper<T>
{
    readonly ConcurrentDictionary<Type, Func<object, List<Parameter>>> _parametersGetters = new();
    readonly List<Parameter> _emptyList = [];
    readonly NpgsqlConnectionFactory _factory;
    readonly SchemaManager _schemaManager;
    readonly Internals.Reflection.Activator _activator;
    readonly ClassMap _classMap;
    readonly Settings _settings;

    /// <summary>
    /// Creates the upsert helper
    /// </summary>
    public UpsertHelper(string connectionString, string tableName = null, string schema = "public", Settings settings = null)
        : this(connectionString, new AutoMapper().GetMap(typeof(T)), tableName, schema, settings)
    {
    }

    /// <summary>
    /// Creates the upsert helper
    /// </summary>
    public UpsertHelper(string connectionString, ClassMap classMap, string tableName = null, string schema = "public", Settings settings = null)
    {
        _factory = new NpgsqlConnectionFactory(connectionString);
        _classMap = classMap ?? throw new ArgumentNullException(nameof(classMap));
        _settings = settings ?? new Settings();

        var upsertTableName = tableName ?? typeof(T).Name.ToLowerInvariant();

        _schemaManager = GetSchemaCreator(schema, upsertTableName);

        _activator = new Internals.Reflection.Activator(typeof(T), _classMap.Properties.Select(p => p.PropertyName));
    }

    /// <summary>
    /// Ensures that the necessary schema is created (i.e. table).
    /// Does NOT detect changes, just skips creation if it finds objects with the known names in the database.
    /// This means that you need to handle migrations yourself
    /// </summary>
    public void CreateSchema(bool createTable = true) => _schemaManager.CreateSchema(createTable);

    public string GetCreateSchemaScript(bool createTable = true)
    {
        var table = _schemaManager.GetCreateSchemaScript();
        return createTable ? table : "";
    }

    /// <summary>
    /// Immediately executes DROP statements for the things you select by setting <paramref name="dropTable"/> to <code>true</code>.
    /// </summary>
    public void DropSchema(bool dropTable = false) => _schemaManager.DropSchema(dropTable);

    public string GetDropSchemaScript(bool dropTable = false)
    {
        var table = _schemaManager.GetDropSchemaScript();
        return dropTable ? table : "";
    }

    /// <summary>
    /// Upserts the given sequence of <typeparamref name="T"/> instances
    /// </summary>
    public async Task UpsertAsync(IEnumerable<T> rows)
    {
        if (rows == null) throw new ArgumentNullException(nameof(rows));

        await using var connection = _factory.OpenNpgsqlConnection();
        await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

        await UpsertAsync(connection, rows, transaction);

        await transaction.CommitAsync();
    }

    /// <summary>
    /// Upserts the given sequence of <typeparamref name="T"/> instances using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
    /// </summary>
    public async Task UpsertAsync(NpgsqlConnection connection, IEnumerable<T> rows, NpgsqlTransaction transaction = null)
    {
        // For now, we'll implement a simple approach using COPY and then ON CONFLICT
        // This is a placeholder implementation to get tests compiling
        var rowsList = rows.ToList();
        if (!rowsList.Any()) return;

        var upsertSql = _schemaManager.GetUpsertSql();
        
        await using var writer = await connection.BeginBinaryImportAsync(upsertSql, cancellationToken: default);
        
        foreach (var row in rowsList)
        {
            await writer.StartRowAsync();
            foreach (var property in _classMap.Properties)
            {
                await property.WriteToAsync(writer, row);
            }
        }
        
        await writer.CompleteAsync();
    }

    /// <summary>
    /// Loads all rows from the database (in a streaming fashion, allows you to traverse all
    /// objects without worrying about memory usage)
    /// </summary>
    public IEnumerable<T> LoadAll()
    {
        using var connection = _factory.OpenNpgsqlConnection();
        using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

        // it's important that we traverse&yield here to avoid premature disposal of the connection/transaction
        foreach (var instance in LoadAll(connection, transaction))
        {
            yield return instance;
        }
    }

    /// <summary>
    /// Loads all rows from the database (in a streaming fashion, allows you to traverse all
    /// objects without worrying about memory usage) using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
    /// </summary>
    public IEnumerable<T> LoadAll(NpgsqlConnection connection, NpgsqlTransaction transaction = null)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandTimeout = _settings.CommandTimeoutSeconds;
        command.CommandType = CommandType.Text;
        command.CommandText = _schemaManager.GetQuery();

        using var reader = command.ExecuteReader();

        var classMapProperties = _classMap.Properties.ToDictionary(p => p.PropertyName);
        var lookup = new DataReaderLookup(reader, classMapProperties);

        while (reader.Read())
        {
            yield return (T)_activator.CreateInstance(lookup);
        }
    }

    /// <summary>
    /// Asynchronously loads all rows from the database (in a streaming fashion, allows you to traverse all
    /// objects without worrying about memory usage) 
    /// </summary>
    public async IAsyncEnumerable<T> LoadAllAsync()
    {
        await using var connection = _factory.OpenNpgsqlConnection();
        await using var transaction = await connection.BeginTransactionAsync(_settings.TransactionIsolationLevel);

        // it's important that we traverse&yield here to avoid premature disposal of the connection/transaction
        await foreach (var instance in LoadAllAsync(connection, transaction))
        {
            yield return instance;
        }
    }

    /// <summary>
    /// Asynchronously loads all rows from the database (in a streaming fashion, allows you to traverse all
    /// objects without worrying about memory usage) using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
    /// </summary>
    public async IAsyncEnumerable<T> LoadAllAsync(NpgsqlConnection connection, NpgsqlTransaction transaction = null)
    {
        await using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandTimeout = _settings.CommandTimeoutSeconds;
        command.CommandType = CommandType.Text;
        command.CommandText = _schemaManager.GetQuery();

        await using var reader = await command.ExecuteReaderAsync();

        var classMapProperties = _classMap.Properties.ToDictionary(p => p.PropertyName);
        var lookup = new DataReaderLookup(reader, classMapProperties);

        while (await reader.ReadAsync())
        {
            yield return (T)_activator.CreateInstance(lookup);
        }
    }

    SchemaManager GetSchemaCreator(string schema, string tableName)
    {
        var properties = _classMap.Properties.ToList();
        var keyProperties = properties.Where(p => p.IsKey);
        var extraCriteria = _classMap.GetExtraCriteria();

        return new SchemaManager(_factory, tableName, keyProperties, properties, schema, extraCriteria);
    }
}