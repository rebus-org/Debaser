using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Debaser.Internals.Query
{
    class Parameter
    {
        static readonly Dictionary<Type, SqlDbType> KnownTypes = new Dictionary<Type, SqlDbType>
        {
            {typeof(string), SqlDbType.NVarChar },
            {typeof(bool), SqlDbType.Bit },
            {typeof(byte), SqlDbType.TinyInt },
            {typeof(short), SqlDbType.SmallInt },
            {typeof(int), SqlDbType.Int },
            {typeof(long), SqlDbType.BigInt },
            {typeof(float), SqlDbType.Float },
            {typeof(double), SqlDbType.Decimal },
            {typeof(decimal), SqlDbType.Decimal },
            {typeof(DateTime), SqlDbType.DateTime2 },
            {typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
            {typeof(Guid), SqlDbType.UniqueIdentifier },
        };

        public string Name { get; }
        public object Value { get; }

        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public void AddTo(SqlCommand command)
        {
            if (Equals(null, Value))
            {
                command.Parameters.AddWithValue(Name, null);
            }
            else
            {
                var sqlDbType = GetSqlDbType();

                command.Parameters.Add(Name, sqlDbType).Value = Value;
            }
        }

        SqlDbType GetSqlDbType()
        {
            SqlDbType sqlDbType;
            var type = Value.GetType();

            if (KnownTypes.TryGetValue(type, out sqlDbType)) return sqlDbType;

            throw new ArgumentException($"Does not know which SqlDbType to use for {type}");
        }
    }
}