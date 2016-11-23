using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Data;
using Debaser.Mapping;
using Debaser.Schema;
using Microsoft.SqlServer.Server;
using Activator = Debaser.Reflection.Activator;

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

        public IEnumerable<T> Load()
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
                            var lookup = new DataReaderLookup(reader);

                            while (reader.Read())
                            {
                                yield return (T)_activator.CreateInstance(lookup);
                            }
                        }
                    }
                }
            }
        }

        IEnumerable<SqlDataRecord> GetData(IEnumerable<T> rows)
        {
            var sqlMetaData = _classMap.GetSqlMetaData();
            var reusableRecord = new SqlDataRecord(sqlMetaData);

            foreach (var row in rows)
            {
                foreach (var property in _classMap.Properties)
                {
                    property.WriteTo(reusableRecord, row);
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
