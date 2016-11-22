using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Mapping;
using Debaser.Schema;

namespace Debaser
{
    public class UpsertHelper<T>
    {
        readonly string _schema;
        readonly string _tableName;
        readonly string _connectionString;
        readonly ClassMap _classMap;
        SchemaCreator _schemaCreator;

        public UpsertHelper(string connectionStringOrConnectionStringName, string tableName = null, string schema = "dbo")
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName];

            _connectionString = connectionStringSettings?.ConnectionString
                                ?? connectionStringOrConnectionStringName;

            _tableName = tableName ?? typeof(T).Name;
            _schema = schema;

            _classMap = new AutoMapper().GetMap(typeof(T));

            _schemaCreator = GetSchemaCreator();
        }

        public void DropSchema()
        {
            _schemaCreator.DropSchema();
        }

        public void CreateSchema()
        {
            _schemaCreator.CreateSchema();
        }

        SchemaCreator GetSchemaCreator()
        {
            var properties = _classMap.Properties.ToList();
            var dataTypeName = $"{_tableName}Type";
            var procedureName = $"{_tableName}Upsert";
            var keyProperties = properties.Where(p => p.IsKey);
            return new SchemaCreator(_connectionString, _tableName, dataTypeName, procedureName, keyProperties, properties, _schema);
        }

        public async Task Upsert(IEnumerable<T> rows)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> Load()
        {
            throw new System.NotImplementedException();
        }
    }
}
