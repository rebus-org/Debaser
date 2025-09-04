using System.Data;
using Postgredebaser.Internals.Values;
using Postgredebaser.Mapping;

namespace Postgredebaser.Internals.Data;

class DataReaderLookup(IDataReader reader, Dictionary<string, ClassMapProperty> properties) : IValueLookup
{
    public object GetValue(string name)
    {
        if (!properties.TryGetValue(name, out var property))
        {
            throw new ArgumentException($"Property '{name}' not found in mapping");
        }

        var ordinal = reader.GetOrdinal(property.ColumnName);
        var value = reader.GetValue(ordinal);

        return property.FromDatabase(value);
    }
}