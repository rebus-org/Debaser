using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Debaser.Internals.Values;
using Debaser.Mapping;

namespace Debaser.Internals.Data
{
    class DataReaderLookup : IValueLookup
    {
        readonly SqlDataReader _reader;
        readonly Dictionary<string, ClassMapProperty> _properties;

        public DataReaderLookup(SqlDataReader reader, Dictionary<string, ClassMapProperty> properties)
        {
            _reader = reader;
            _properties = new Dictionary<string, ClassMapProperty>(properties, StringComparer.CurrentCultureIgnoreCase);
        }

        public object GetValue(string name, Type desiredType)
        {
            var ordinal = _reader.GetOrdinal(GetColumnName(name));
            var value = _reader.GetValue(ordinal);

            var property = GetProperty(name);

            try
            {
                return property.FromDatabase(value == DBNull.Value ? null : value);
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not get value {name}", exception);
            }
        }

        string GetColumnName(string name)
        {
            try
            {
                return _properties[name].ColumnName;
            }
            catch (Exception exception)
            {
                throw new KeyNotFoundException($"Could not find column name corresponding to property/ctor parameter named '{name}'", exception);
            }
        }

        ClassMapProperty GetProperty(string name)
        {
            try
            {
                return _properties[name];
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not get property named {name} - have these properties: {string.Join(", ", _properties.Select(p => p.Key))}", exception);
            }
        }
    }
}