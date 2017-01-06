using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Debaser.Internals.Ex;
using Debaser.Mapping;

// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Internals.Schema
{
    class SchemaManager
    {
        readonly string _connectionString;
        readonly string _tableName;
        readonly string _dataTypeName;
        readonly string _sprocName;
        readonly string _schema;
        readonly List<ClassMapProperty> _mutableProperties;
        readonly List<ClassMapProperty> _keyProperties;
        readonly List<ClassMapProperty> _properties;

        public SchemaManager(string connectionString, string tableName, string dataTypeName, string sprocName, IEnumerable<ClassMapProperty> keyProperties, IEnumerable<ClassMapProperty> properties, string schema = "dbo")
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _dataTypeName = dataTypeName;
            _sprocName = sprocName;
            _schema = schema;
            _keyProperties = keyProperties.ToList();
            _properties = properties.ToList();

            _mutableProperties = _properties.Except(_keyProperties).ToList();
        }

        public string SprocName => _sprocName;

        public string DataTypeName => _dataTypeName;

        public void CreateSchema()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    ExecuteStatement(connection, transaction, $@"

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE [name]='{_schema}')
	EXEC('CREATE SCHEMA [{_schema}]')

");

                    var schemaId = ExecuteQuery(connection, transaction, $@"

SELECT [schema_id] FROM sys.schemas WHERE [name]='{_schema}'

");

                    ExecuteStatement(connection, transaction, $@"

IF NOT EXISTS (SELECT * FROM sys.objects WHERE [name]='{_tableName}' AND [type]='U' AND [schema_id]={schemaId})
    CREATE TABLE [{_schema}].[{_tableName}] (
{GetColumnDefinitionSql(8)},
        PRIMARY KEY({string.Join(", ", _keyProperties.Select(p => $"[{p.ColumnName}]"))})
    )

");

                    ExecuteStatement(connection, transaction, $@"

IF NOT EXISTS (SELECT * FROM sys.types WHERE [name]='{_dataTypeName}' AND [is_user_defined] = 1 AND [schema_id]={schemaId})
    CREATE TYPE [{_schema}].[{_dataTypeName}] AS TABLE (
{GetColumnDefinitionSql(8)}
    )

");

                    ExecuteStatement(connection, transaction, $@"

IF NOT EXISTS (SELECT * FROM sys.objects WHERE [name]='{_sprocName}' AND [type]='P' AND [schema_id]={schemaId})
    EXEC('CREATE PROCEDURE [{_schema}].[{_sprocName}] AS BEGIN SET NOCOUNT ON; END')

");

                    ExecuteStatement(connection, transaction, $@"

ALTER PROCEDURE [{_schema}].[{_sprocName}] (
    @data [{_schema}].[{_dataTypeName}] READONLY
)
AS
BEGIN
    SET NOCOUNT ON;  

    MERGE INTO [{_schema}].[{_tableName}] AS T USING @data AS S ON {GetKeyComparison()}
    
    WHEN MATCHED THEN 
       UPDATE SET
{GetUpdateSql(10)}

    WHEN NOT MATCHED THEN
      INSERT VALUES (
{GetInsertSql(10)}
      )

    ;
    
END

");

                    transaction.Commit();
                }
            }
        }

        static int ExecuteQuery(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;
                return (int)command.ExecuteScalar();
            }
        }

        public void DropSchema()
        {
            const int objectNotFound = 3701;
            const int typeNotFound = 218;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteStatement(connection, transaction, $@"DROP PROCEDURE [{_schema}].[{_sprocName}]", wrapException: false);

                        transaction.Commit();
                    }
                    catch (SqlException exception) when (exception.Number == objectNotFound)
                    {
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteStatement(connection, transaction, $@"DROP TYPE [{_schema}].[{_dataTypeName}]", wrapException: false);

                        transaction.Commit();
                    }
                    catch (SqlException exception) when (exception.Number == typeNotFound)
                    {
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteStatement(connection, transaction, $@"DROP TABLE [{_schema}].[{_tableName}]", wrapException: false);

                        transaction.Commit();
                    }
                    catch (SqlException exception) when (exception.Number == objectNotFound)
                    {
                    }
                }
            }
        }

        public string GetQuery()
        {
            var columnList = string.Join("," + Environment.NewLine, _properties.Select(p => p.ColumnName).Indented(4));

            return $@"

SELECT 
{columnList}
FROM [{_schema}].[{_tableName}]

";
        }

        string GetUpdateSql(int indentation)
        {
            return string.Join("," + Environment.NewLine,
                _mutableProperties.Select(p => $"[T].[{p.ColumnName}] = [S].[{p.ColumnName}]").Indented(indentation));
        }

        string GetInsertSql(int indentation)
        {
            return string.Join(", " + Environment.NewLine,
                _properties.Select(p => $"[S].[{p.ColumnName}]").Indented(indentation));
        }

        string GetColumnDefinitionSql(int indentation)
        {
            return string.Join("," + Environment.NewLine,
                _properties.Select(p => $"{p.GetColumnDefinition()}").Indented(indentation));
        }

        string GetKeyComparison()
        {
            return string.Join(" AND ", _keyProperties.Select(p => $"[T].[{p.ColumnName}] = [S].[{p.ColumnName}]"));
        }

        static void ExecuteStatement(SqlConnection connection, SqlTransaction transaction, string sql, bool wrapException = true)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                if (!wrapException) throw;

                throw new ApplicationException($@"Could not execute SQL: 

{sql}
", exception);
            }
        }
    }
}
