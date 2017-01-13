using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Internals.Data;
using Debaser.Internals.Query;
using Debaser.Internals.Schema;
using Debaser.Mapping;
using Microsoft.SqlServer.Server;
using Activator = Debaser.Internals.Reflection.Activator;

namespace Debaser
{
    public class UpsertHelper<T>
    {
        readonly Activator _activator = new Activator(typeof(T));
        readonly SchemaManager _schemaManager;
        readonly string _connectionString;
        readonly ClassMap _classMap;

        public UpsertHelper(string connectionStringOrConnectionStringName, string tableName = null, string schema = "dbo")
            : this(connectionStringOrConnectionStringName, new AutoMapper().GetMap(typeof(T)), tableName, schema)
        {
        }

        public UpsertHelper(string connectionStringOrConnectionStringName, ClassMap classMap, string tableName = null, string schema = "dbo")
        {
            if (connectionStringOrConnectionStringName == null) throw new ArgumentNullException(nameof(connectionStringOrConnectionStringName));
            if (classMap == null) throw new ArgumentNullException(nameof(classMap));

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];

            _connectionString = connectionStringSettings?.ConnectionString
                                ?? connectionStringOrConnectionStringName;

            _classMap = classMap;

            var upsertTableName = tableName ?? typeof(T).Name;
            var dataTypeName = $"{upsertTableName}Type";
            var procedureName = $"{upsertTableName}Upsert";

            _schemaManager = GetSchemaCreator(schema, upsertTableName, dataTypeName, procedureName);
        }

        public void DropSchema()
        {
            _schemaManager.DropSchema();
        }

        public void CreateSchema()
        {
            _schemaManager.CreateSchema();
        }

        public async Task Upsert(IEnumerable<T> rows)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = _schemaManager.SprocName;

                        var parameter = command.Parameters.AddWithValue("data", GetData(rows));
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = _schemaManager.DataTypeName;

                        await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        public IEnumerable<T> LoadAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.Text;
                        command.CommandText = _schemaManager.GetQuery();

                        using (var reader = command.ExecuteReader())
                        {
                            var classMapProperties = _classMap.Properties.ToDictionary(p => p.Name);
                            var lookup = new DataReaderLookup(reader, classMapProperties);

                            while (reader.Read())
                            {
                                yield return (T)_activator.CreateInstance(lookup);
                            }
                        }
                    }
                }
            }
        }

        public async Task DeleteWhere(string criteria, object args = null)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandTimeout = 120;
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
            }
        }

        public async Task<List<T>> LoadWhere(string criteria, object args = null)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            var results = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandTimeout = 120;
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
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var classMapProperties = _classMap.Properties.ToDictionary(p => p.Name);
                                var lookup = new DataReaderLookup(reader, classMapProperties);

                                while (reader.Read())
                                {
                                    var instance = (T) _activator.CreateInstance(lookup);

                                    results.Add(instance);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new ApplicationException($"Could not execute SQL {querySql}", exception);
                        }
                    }
                }
            }

            return results;
        }

        List<Parameter> GetParameters(object args)
        {
            if (args == null) return new List<Parameter>();

            var properties = args.GetType().GetProperties();

            return properties
                .Select(p => new Parameter(p.Name, p.GetValue(args)))
                .ToList();
        }

        IEnumerable<SqlDataRecord> GetData(IEnumerable<T> rows)
        {
            var sqlMetaData = _classMap.GetSqlMetaData();
            var reusableRecord = new SqlDataRecord(sqlMetaData);

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
            }
        }

        SchemaManager GetSchemaCreator(string schema, string tableName, string dataTypeName, string procedureName)
        {
            var properties = _classMap.Properties.ToList();
            var keyProperties = properties.Where(p => p.IsKey);

            return new SchemaManager(_connectionString, tableName, dataTypeName, procedureName, keyProperties, properties, schema);
        }
    }
}
