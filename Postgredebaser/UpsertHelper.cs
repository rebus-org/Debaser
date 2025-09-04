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

        // Validate that at least one key property exists
        if (!_classMap.Properties.Any(p => p.IsKey))
        {
            throw new ArgumentException($"Could not find any properties marked with [DebaserKey] on {typeof(T)} - you need to have at least one key property");
        }

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
        var rowsList = rows.ToList();
        if (!rowsList.Any()) return;

        var upsertSql = _schemaManager.GetUpsertSql();
        
        foreach (var row in rowsList)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = upsertSql;
            command.CommandTimeout = _settings.CommandTimeoutSeconds;
            
            // Add parameters for each property
            var parameterIndex = 1;
            foreach (var property in _classMap.Properties)
            {
                var value = property.ToDatabase(GetPropertyValue(row, property.PropertyName));
                var parameter = new NpgsqlParameter($"param{parameterIndex++}", value ?? DBNull.Value);
                parameter.NpgsqlDbType = property.ColumnInfo.NpgsqlDbType;
                command.Parameters.Add(parameter);
            }
            
            await command.ExecuteNonQueryAsync();
        }
    }

    object GetPropertyValue(object obj, string propertyName)
    {
        var accessor = TypeAccessor.Create(obj.GetType());
        return accessor[obj, propertyName];
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

    List<Parameter> GetParameters(object args)
    {
        if (args == null) return _emptyList;

        var getter = _parametersGetters.GetOrAdd(args.GetType(), GetGetter);

        return getter(args);
    }

    static Func<object, List<Parameter>> GetGetter(Type type)
    {
        if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
        {
            return args =>
            {
                var dictionary = (IDictionary<string, object>) args;

                return dictionary
                    .Select(kvp => new Parameter(kvp.Key, kvp.Value))
                    .ToList();
            };
        }

        var accessor = TypeAccessor.Create(type);
        var members = accessor.GetMembers();

        return args =>
        {
            var parameters = new List<Parameter>(members.Count);

            for (var index = 0; index < members.Count; index++)
            {
                var member = members[index];
                var name = member.Name;

                parameters.Add(new Parameter(name, accessor[args, name]));
            }

            return parameters;
        };
    }

    /// <summary>
    /// Loads all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
    /// <code>"columnname" = @someValue</code> where the accompanying <paramref name="args"/> would be something like
    /// <code>new { someValue = "hej" }</code>.
    /// <paramref name="args"/> can also be a <code>Dictionary&lt;string, object&gt;</code>
    /// </summary>
    public async Task<IReadOnlyList<T>> LoadWhereAsync(string criteria, object args = null)
    {
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));

        await using var connection = _factory.OpenNpgsqlConnection();
        await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

        return await LoadWhereAsync(connection, criteria, args, transaction);
    }

    /// <summary>
    /// Loads all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
    /// <code>"columnname" = @someValue</code> where the accompanying <paramref name="args"/> would be something like
    /// <code>new { someValue = "hej" }</code>  using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
    /// <paramref name="args"/> can also be a <code>Dictionary&lt;string, object&gt;</code>
    /// </summary>
    public async Task<IReadOnlyList<T>> LoadWhereAsync(NpgsqlConnection connection, string criteria, object args = null, NpgsqlTransaction transaction = null)
    {
        var results = new List<T>();

        await using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandTimeout = _settings.CommandTimeoutSeconds;
        command.CommandType = CommandType.Text;

        var querySql = _schemaManager.GetQuery(criteria);
        var parameters = GetParameters(args);

        if (parameters.Any())
        {
            foreach (var parameter in parameters)
            {
                parameter.AddTo(command);
            }
        }

        command.CommandText = querySql;

        try
        {
            await using var reader = await command.ExecuteReaderAsync();

            var classMapProperties = _classMap.Properties.ToDictionary(p => p.PropertyName);
            var lookup = new DataReaderLookup(reader, classMapProperties);

            while (reader.Read())
            {
                var instance = (T)_activator.CreateInstance(lookup);

                results.Add(instance);
            }
        }
        catch (Exception exception)
        {
            throw new ApplicationException($"Could not execute SQL {querySql}", exception);
        }

        return results;
    }

    /// <summary>
    /// Deletes all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
    /// <code>"columnname" = @someValue</code> where the accompanying <paramref name="args"/> would be something like
    /// <code>new { someValue = "hej" }</code>
    /// <paramref name="args"/> can also be a <code>Dictionary&lt;string, object&gt;</code>
    /// </summary>
    public async Task DeleteWhereAsync(string criteria, object args = null)
    {
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));

        await using var connection = _factory.OpenNpgsqlConnection();
        await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);
        await DeleteWhereAsync(connection, criteria, args, transaction);

        transaction.Commit();
    }

    /// <summary>
    /// Deletes all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
    /// <code>"columnname" = @someValue</code> where the accompanying <paramref name="args"/> would be something like
    /// <code>new { someValue = "hej" }</code> using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
    /// <paramref name="args"/> can also be a <code>Dictionary&lt;string, object&gt;</code>
    /// </summary>
    public async Task DeleteWhereAsync(NpgsqlConnection connection, string criteria, object args, NpgsqlTransaction transaction = null)
    {
        await using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandTimeout = _settings.CommandTimeoutSeconds;
        command.CommandType = CommandType.Text;

        var querySql = _schemaManager.GetDeleteCommand(criteria);
        var parameters = GetParameters(args);

        if (parameters.Any())
        {
            foreach (var parameter in parameters)
            {
                parameter.AddTo(command);
            }
        }

        command.CommandText = querySql;

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            throw new ApplicationException($"Could not execute SQL {querySql}", exception);
        }
    }
}