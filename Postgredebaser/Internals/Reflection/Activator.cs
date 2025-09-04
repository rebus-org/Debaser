using FastMember;
using Postgredebaser.Internals.Values;

namespace Postgredebaser.Internals.Reflection;

class Activator
{
    readonly Func<IValueLookup, object> _objectActivator;

    public Activator(Type type, IEnumerable<string> propertyNames)
    {
        var accessor = TypeAccessor.Create(type);
        var properties = propertyNames.ToList();

        _objectActivator = lookup =>
        {
            var instance = accessor.CreateNew();

            foreach (var propertyName in properties)
            {
                var value = lookup.GetValue(propertyName);

                accessor[instance, propertyName] = value;
            }

            return instance;
        };
    }

    public object CreateInstance(IValueLookup lookup) => _objectActivator(lookup);
}