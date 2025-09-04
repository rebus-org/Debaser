using Debaser.Core.Internals.Values;
using FastMember;

namespace Debaser.Core.Internals.Reflection;

public class Activator
{
    readonly Func<IValueLookup, object> _creationFunction;

    public Activator(Type type, IEnumerable<string> includedProperties)
    {
        var propertyNames = new HashSet<string>(includedProperties);

        _creationFunction = HasDefaultConstructor(type)
            ? GetPropertyCreator(type, propertyNames)
            : GetConstructorCreator(type, propertyNames);
    }

    public object CreateInstance(IValueLookup valueLookup)
    {
        return _creationFunction(valueLookup);
    }

    static Func<IValueLookup, object> GetPropertyCreator(Type type, HashSet<string> includedProperties)
    {
        var properties = type.GetProperties()
            .Where(p => p.SetMethod != null)
            .Where(p => includedProperties.Contains(p.Name))
            .ToArray();

        var accessor = TypeAccessor.Create(type);
            
        return lookup =>
        {
            var instance = System.Activator.CreateInstance(type);

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var value = lookup.GetValue(propertyName, property.PropertyType);
                try
                {
                    accessor[instance, propertyName] = value;
                }
                catch (Exception exception)
                {
                    throw new ApplicationException($"Could not set value of property {propertyName} to {value}", exception);
                }
            }

            return instance;
        };
    }

    static Func<IValueLookup, object> GetConstructorCreator(Type type, HashSet<string> includedProperties)
    {
        var parameters = type.GetConstructors().Single()
            .GetParameters()
            .ToArray();

        return lookup =>
        {
            var parameterValues = parameters
                .Select(parameter =>
                {
                    var titleCase = Capitalize(parameter.Name);

                    if (includedProperties.Contains(titleCase))
                    {
                        return lookup.GetValue(parameter.Name, parameter.ParameterType);
                    }

                    return null;
                })
                .ToArray();

            try
            {
                var instance = System.Activator.CreateInstance(type, parameterValues);

                return instance;
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not create instance of {type} with ctor arguments {string.Join(", ", parameterValues)}", exception);
            }
        };
    }

    static string Capitalize(string parameterName)
    {
        return parameterName.Length > 1 
            ? $"{char.ToUpper(parameterName[0])}{parameterName.Substring(1)}" 
            : $"{char.ToUpper(parameterName[0])}";
    }

    static bool HasDefaultConstructor(Type type)
    {
        var constructors = type.GetConstructors();

        if (constructors.Length > 1)
        {
            throw new ArgumentException($@"Cannot use {type} in the activator because it has more than one constructor. Please supply either 

a) a constructor with named parameters matching the type's properties, or 
b) a default constructor and properties with setters,

this way making it possible in an unambiguous way for Debaser to instantiate instances of {type}.");
        }

        var ctor = constructors.Single();

        return ctor.GetParameters().Length == 0;
    }
}