using System.Data;
using Postgredebaser.Internals.Values;
using Postgredebaser.Mapping;

namespace Postgredebaser.Internals.Data;

class DataReaderLookup(IDataReader reader, Dictionary<string, ClassMapProperty> properties) : IValueLookup
{
    readonly Dictionary<string, ClassMapProperty> _properties = new(properties, StringComparer.CurrentCultureIgnoreCase);

    public object GetValue(string name)
    {
        var property = GetProperty(name);
        var ordinal = reader.GetOrdinal(property.ColumnName);
        var value = reader.GetValue(ordinal);

        try
        {
            return property.FromDatabase(value);
        }
        catch (Exception exception)
        {
            throw new ApplicationException($"Could not get value {name}", exception);
        }
    }

    public object GetValue(string name, Type desiredType)
    {
        // For the shared Activator interface - just delegate to our existing method
        // The desiredType is handled by the property mapping logic
        return GetValue(name);
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