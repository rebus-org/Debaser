using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Debaser.Internals.Values;
using Debaser.Mapping;

namespace Debaser.Internals.Data;

class DataReaderLookup(SqlDataReader reader, Dictionary<string, ClassMapProperty> properties) : IValueLookup
{
    readonly Dictionary<string, ClassMapProperty> _properties = new(properties, StringComparer.CurrentCultureIgnoreCase);

    public object GetValue(string name, Type desiredType)
    {
        var ordinal = reader.GetOrdinal(name);
        var value = reader.GetValue(ordinal);

        var property = GetProperty(name);

        try
        {
            return property.FromDatabase(value);
        }
        catch (Exception exception)
        {
            throw new ApplicationException($"Could not get value {name}", exception);
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