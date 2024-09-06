using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Debaser.Internals.Query;

class Parameter(string name, object value)
{
    static readonly Dictionary<Type, SqlDbType> KnownTypes = new()
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

    public string Name { get; } = name;
    public object Value { get; } = value;

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
        var type = Value.GetType();

        if (KnownTypes.TryGetValue(type, out var sqlDbType)) return sqlDbType;

        throw new ArgumentException($@"Don't not know which SqlDbType to use for value {Value} of type {type}. Please use one of the supported types

{string.Join(Environment.NewLine, KnownTypes.Select(kvp => $"* {kvp.Key} => {kvp.Value}"))}

or map the value to be saved in the database to another type, either by

* adding a [DebaserMapper(...)] attribute to the property, or
* add a class map with an appropriate mapper function in the ClassMapProperty for this field

this way enabling mapping the column to/from the database.");
    }
}