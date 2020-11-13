using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Internals.Data;
using Debaser.Internals.Ex;
using Debaser.Internals.Exceptions;
using Debaser.Internals.Query;
using Debaser.Internals.Schema;
using Debaser.Internals.Sql;
using Debaser.Mapping;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Activator = Debaser.Internals.Reflection.Activator;
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable UnusedMember.Global

namespace Debaser
{
    /// <summary>
    /// This is the UpsertHelper. <code>new</code> up an instance of this guy and start messing around with your data
    /// </summary>
    public class UpsertHelper<T>
    {
        readonly ConcurrentDictionary<Type, Func<object, List<Parameter>>> _parametersGetters = new ConcurrentDictionary<Type, Func<object, List<Parameter>>>();
        readonly List<Parameter> _emptyList = new List<Parameter>();
        readonly SqlConnectionFactory _factory;
        readonly SchemaManager _schemaManager;
        readonly Activator _activator;
        readonly ClassMap _classMap;
        readonly Settings _settings;

        /// <summary>
        /// Creates the upsert helper
        /// </summary>
        public UpsertHelper(string connectionString, string tableName = null, string typeName = null, string procName = null, string schema = "dbo", Settings settings = null)
            : this(connectionString, new AutoMapper().GetMap(typeof(T)), tableName, typeName, procName, schema, settings)
        {
        }

        /// <summary>
        /// Creates the upsert helper
        /// </summary>
        public UpsertHelper(string connectionString, ClassMap classMap, string tableName = null, string typeName = null, string procName = null, string schema = "dbo", Settings settings = null)
        {
            _factory = new SqlConnectionFactory(connectionString);
            _classMap = classMap ?? throw new ArgumentNullException(nameof(classMap));
            _settings = settings ?? new Settings();

            var upsertTableName = tableName ?? typeof(T).Name;
            var dataTypeName = typeName ?? $"{upsertTableName}Type";
            var procedureName = procName ?? $"{upsertTableName}Upsert";

            _schemaManager = GetSchemaCreator(schema, upsertTableName, dataTypeName, procedureName);

            _activator = new Activator(typeof(T), _classMap.Properties.Select(p => p.PropertyName));
        }

        /// <summary>
        /// Ensures that the necessary schema is created (i.e. table, custom data type, and stored procedure).
        /// Does NOT detect changes, just skips creation if it finds objects with the known names in the database.
        /// This means that you need to handle migrations yourself
        /// </summary>
        public void CreateSchema(bool createProcedure = true, bool createType = true, bool createTable = true) => _schemaManager.CreateSchema(createProcedure, createType, createTable);

        public string GetCreateSchemaScript(bool createProcedure = true, bool createType = true, bool createTable = true)
        {
            var (procedure, type, table) = _schemaManager.GetCreateSchemaScript();

            return JoinScripts(new[]
            {
                createTable ? table : null,
                createType ? type : null,
                createProcedure ? procedure : null,
            });
        }

        /// <summary>
        /// Immediately executes DROP statements for the things you select by setting <paramref name="dropProcedure"/>,
        /// <paramref name="dropType"/>, and/or <paramref name="dropTable"/> to <code>true</code>.
        /// </summary>
        public void DropSchema(bool dropProcedure = false, bool dropType = false, bool dropTable = false) => _schemaManager.DropSchema(dropProcedure, dropType, dropTable);

        public string GetDropSchemaScript(bool dropProcedure = false, bool dropType = false, bool dropTable = false)
        {
            var (procedure, type, table) = _schemaManager.GetDropSchemaScript();

            return JoinScripts(new[]
            {
                dropProcedure ? procedure : null,
                dropType ? type : null,
                dropTable ? table : null,
            });
        }

        static string JoinScripts(IEnumerable<string> scripts)
        {
            var scriptsToInclude = scripts
                .Where(script => script != null)
                .Select(script => script.TrimEmptyLines())
                .Where(script => !string.IsNullOrWhiteSpace(script));

            return string.Join(@"

GO

",
                scriptsToInclude);
        }

        /// <summary>
        /// Upserts the given sequence of <typeparamref name="T"/> instances
        /// </summary>
        public async Task UpsertAsync(IEnumerable<T> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            await using var connection = _factory.OpenSqlConnection();
            await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

            await UpsertAsync(connection, rows, transaction);

            transaction.Commit();
        }

        /// <summary>
        /// Upserts the given sequence of <typeparamref name="T"/> instances using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
        /// </summary>
        public async Task UpsertAsync(SqlConnection connection, IEnumerable<T> rows, SqlTransaction transaction = null)
        {
            await using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandTimeout = _settings.CommandTimeoutSeconds;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = _schemaManager.SprocName;

            var parameter = command.Parameters.AddWithValue("data", GetData(rows));
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = _schemaManager.DataTypeName;

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (EmptySequenceException)
            {
            }
        }

        /// <summary>
        /// Loads all rows from the database (in a streaming fashion, allows you to traverse all
        /// objects without worrying about memory usage)
        /// </summary>
        public IEnumerable<T> LoadAll()
        {
            using var connection = _factory.OpenSqlConnection();
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
        public IEnumerable<T> LoadAll(SqlConnection connection, SqlTransaction transaction = null)
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
            await using var connection = _factory.OpenSqlConnection();
            await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

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
        public async IAsyncEnumerable<T> LoadAllAsync(SqlConnection connection, SqlTransaction transaction = null)
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

        /// <summary>
        /// Deletes all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
        /// <code>[someColumn] = @someValue</code> where the accompanying <paramref name="args"/> would be something like
        /// <code>new { someValue = "hej" }</code>
        /// </summary>
        public async Task DeleteWhereAsync(string criteria, object args = null)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            await using var connection = _factory.OpenSqlConnection();
            await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);
            await DeleteWhereAsync(connection, criteria, args, transaction);

            transaction.Commit();
        }

        /// <summary>
        /// Deletes all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
        /// <code>[someColumn] = @someValue</code> where the accompanying <paramref name="args"/> would be something like
        /// <code>new { someValue = "hej" }</code> using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
        /// </summary>
        public async Task DeleteWhereAsync(SqlConnection connection, string criteria, object args, SqlTransaction transaction = null)
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

        /// <summary>
        /// Loads all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
        /// <code>[someColumn] = @someValue</code> where the accompanying <paramref name="args"/> would be something like
        /// <code>new { someValue = "hej" }</code>
        /// </summary>
        public async Task<IReadOnlyList<T>> LoadWhereAsync(string criteria, object args = null)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            await using var connection = _factory.OpenSqlConnection();
            await using var transaction = connection.BeginTransaction(_settings.TransactionIsolationLevel);

            return await LoadWhereAsync(connection, criteria, args, transaction);
        }

        /// <summary>
        /// Loads all rows that match the given criteria. The <paramref name="criteria"/> must be specified on the form
        /// <code>[someColumn] = @someValue</code> where the accompanying <paramref name="args"/> would be something like
        /// <code>new { someValue = "hej" }</code>  using the given <paramref name="connection"/> (possibly also enlisting the command in the given <paramref name="transaction"/>)
        /// </summary>
        public async Task<IReadOnlyList<T>> LoadWhereAsync(SqlConnection connection, string criteria, object args = null, SqlTransaction transaction = null)
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

        List<Parameter> GetParameters(object args)
        {
            if (args == null) return _emptyList;

            var getter = _parametersGetters.GetOrAdd(args.GetType(), GetGetter);

            return getter(args);
        }

        static Func<object, List<Parameter>> GetGetter(Type type)
        {
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

        IEnumerable<SqlDataRecord> GetData(IEnumerable<T> rows)
        {
            var sqlMetaData = _classMap.GetSqlMetaData();
            var reusableRecord = new SqlDataRecord(sqlMetaData);
            var didYieldRows = false;

            foreach (var row in rows)
            {
                foreach (var property in _classMap.Properties)
                {
                    try
                    {
                        property.WriteTo(reusableRecord, row);
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException($"Could not write property {property} of row {row}", exception);
                    }
                }

                yield return reusableRecord;

                didYieldRows = true;
            }

            // sorry - but we need to handle this somehow, and we don't know that the sequence was empty until we have tried to run it through
            if (!didYieldRows)
            {
                throw new EmptySequenceException();
            }
        }

        SchemaManager GetSchemaCreator(string schema, string tableName, string dataTypeName, string procedureName)
        {
            var properties = _classMap.Properties.ToList();
            var keyProperties = properties.Where(p => p.IsKey);
            var extraCriteria = _classMap.GetExtraCriteria();

            return new SchemaManager(_factory, tableName, dataTypeName, procedureName, keyProperties, properties, schema, extraCriteria);
        }
    }
}
