using System.Reflection;
using Debaser.Core.Attributes;
using NpgsqlTypes;
using Postgredebaser.Attributes;

namespace Postgredebaser.Mapping;

/// <summary>
/// Helper that can generate a <see cref="ClassMap"/> for the PostgreSQL <see cref="UpsertHelper{T}"/> to use
/// </summary>
public class AutoMapper
{
    /// <summary>
    /// Gets an automatically generated map from the given <paramref name="type"/>
    /// </summary>
    public ClassMap GetMap(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        try
        {
            var properties = GetProperties(type).ToList();

            return new ClassMap(type, properties);
        }
        catch (Exception exception)
        {
            throw new ArgumentException($"Could not auto-map {type}", exception);
        }
    }

    static IEnumerable<ClassMapProperty> GetProperties(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        Func<object, object> DefaultFromDatabase() => obj => obj == DBNull.Value ? null : obj;
        Func<object, object> DefaultToDatabase() => obj => obj;

        var properties = type.GetProperties()
            .Where(property => !property.GetCustomAttributes<DebaserIgnoreAttribute>().Any())
            .Select(property =>
            {
                var propertyName = property.Name;
                var columnInfo = GetColumnInfo(property);
                var columnName = property.Name.ToLowerInvariant(); // PostgreSQL convention
                var isKey = property.GetCustomAttributes<DebaserKeyAttribute>().Any();

                var toDatabase = columnInfo.CustomToDatabase ?? DefaultToDatabase();
                var fromDatabase = columnInfo.CustomFromDatabase ?? DefaultFromDatabase();

                return new ClassMapProperty(
                    propertyName: propertyName,
                    columnName: columnName,
                    columnInfo: columnInfo,
                    isKey: isKey,
                    toDatabase: toDatabase,
                    fromDatabase: fromDatabase
                );
            })
            .ToList();

        if (!properties.Any(p => p.IsKey))
        {
            var defaultKeyProperty = properties.FirstOrDefault(p => string.Equals("Id", p.PropertyName));

            if (defaultKeyProperty == null)
            {
                throw new ArgumentException(@"Could not find property named 'Id' - please either 

* add a property called 'Id', or
* decorate one or more properties with the [DebaserKey] attribute, or
* pass a class map to the UpsertHelper ctor with one or more properties designated as keys

so Debaser will know how to identity each row.");
            }

            defaultKeyProperty.MakeKey();
        }

        return properties;
    }

    static ColumnInfo GetColumnInfo(PropertyInfo property)
    {
        var typeMapping = new Dictionary<Type, ColumnInfo>
        {
            {typeof(string), new ColumnInfo(NpgsqlDbType.Text)},
            {typeof(int), new ColumnInfo(NpgsqlDbType.Integer)},
            {typeof(int?), new ColumnInfo(NpgsqlDbType.Integer)},
            {typeof(long), new ColumnInfo(NpgsqlDbType.Bigint)},
            {typeof(long?), new ColumnInfo(NpgsqlDbType.Bigint)},
            {typeof(bool), new ColumnInfo(NpgsqlDbType.Boolean)},
            {typeof(bool?), new ColumnInfo(NpgsqlDbType.Boolean)},
            {typeof(DateTime), new ColumnInfo(NpgsqlDbType.Timestamp)},
            {typeof(DateTime?), new ColumnInfo(NpgsqlDbType.Timestamp)},
            {typeof(DateTimeOffset), new ColumnInfo(
                NpgsqlDbType.TimestampTz,
                customToDatabase: obj => obj is DateTimeOffset dto ? dto.UtcDateTime : obj,
                customFromDatabase: obj => obj is DateTime dt ? new DateTimeOffset(dt, TimeSpan.Zero) : obj
            )},
            {typeof(DateTimeOffset?), new ColumnInfo(
                NpgsqlDbType.TimestampTz,
                customToDatabase: obj => obj is DateTimeOffset dtoValue ? dtoValue.UtcDateTime : obj,
                customFromDatabase: obj => obj == DBNull.Value || obj == null ? (DateTimeOffset?)null : obj is DateTime dt ? new DateTimeOffset(dt, TimeSpan.Zero) : obj
            )},
            {typeof(decimal), new ColumnInfo(NpgsqlDbType.Numeric)},
            {typeof(decimal?), new ColumnInfo(NpgsqlDbType.Numeric)},
            {typeof(double), new ColumnInfo(NpgsqlDbType.Double)},
            {typeof(double?), new ColumnInfo(NpgsqlDbType.Double)},
            {typeof(float), new ColumnInfo(NpgsqlDbType.Real)},
            {typeof(float?), new ColumnInfo(NpgsqlDbType.Real)},
            {typeof(Guid), new ColumnInfo(NpgsqlDbType.Uuid)},
            {typeof(Guid?), new ColumnInfo(NpgsqlDbType.Uuid)},
        };

        var debaserMapperAttribute = property.GetCustomAttribute<DebaserMapperAttribute>();
        var debaserTypeAttribute = property.GetCustomAttribute<DebaserNpgsqlTypeAttribute>();

        if (debaserTypeAttribute != null && debaserMapperAttribute != null)
        {
            throw new InvalidOperationException($"Found both [DebaserNpgsqlType(...)] and [DebaserMapper(...)] on property {property.Name} of {property.DeclaringType}, but it's only possible to use one of them.");
        }

        if (debaserTypeAttribute != null)
        {
            return new ColumnInfo(debaserTypeAttribute.NpgsqlDbType, debaserTypeAttribute.Size, debaserTypeAttribute.AltSize);
        }

        if (debaserMapperAttribute != null)
        {
            var mapperType = debaserMapperAttribute.DebaserMapperType;

            if (!typeof(IDebaserMapper).IsAssignableFrom(mapperType))
            {
                throw new InvalidOperationException($"The mapper {mapperType} does not implement {typeof(IDebaserMapper)}");
            }

            var mapper = (IDebaserMapper)Activator.CreateInstance(mapperType);

            return new ColumnInfo(
                npgsqlDbType: mapper.NpgsqlDbType,
                size: mapper.SizeOrNull,
                additionalSize: mapper.AdditionalSizeOrNull,
                customToDatabase: mapper.ToDatabase,
                customFromDatabase: mapper.FromDatabase
            );
        }

        if (typeMapping.TryGetValue(property.PropertyType, out var info))
        {
            return info;
        }

        throw new ArgumentException($"Could not figure out how to map property {property.Name} of type {property.PropertyType} - please configure either a [DebaserNpgsqlType] or a [DebaserMapper]");
    }
}