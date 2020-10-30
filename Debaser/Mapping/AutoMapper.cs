using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Debaser.Attributes;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Debaser.Mapping
{
    /// <summary>
    /// Helper that can generate a <see cref="ClassMap"/> for the <see cref="UpsertHelper{T}"/> to use
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

            var properties = type.GetProperties()
                .Where(property => !property.GetCustomAttributes<DebaserIgnoreAttribute>().Any())
                .Select(property =>
                {
                    var propertyName = property.Name;
                    var columns = GetColumnInfo(property);
                    var columnName = property.Name;
                    var isKey = property.GetCustomAttributes<DebaserKeyAttribute>().Any();

                    return new ClassMapProperty(propertyName, columns, columnName, isKey, property);
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

        static IEnumerable<ColumnInfo> GetColumnInfo(PropertyInfo property)
        {
            var defaultDbTypes = new Dictionary<Type, ColumnInfo>
            {
                {typeof(bool), new ColumnInfo(SqlDbType.Bit)},
                {typeof(byte), new ColumnInfo(SqlDbType.TinyInt)},
                {typeof(short), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(int), new ColumnInfo(SqlDbType.Int)},
                {typeof(long), new ColumnInfo(SqlDbType.BigInt)},

                {typeof(decimal), new ColumnInfo(SqlDbType.Decimal)},
                {typeof(double), new ColumnInfo(SqlDbType.Float)},
                {typeof(float), new ColumnInfo(SqlDbType.Real)},

                {typeof(string), new ColumnInfo(SqlDbType.NVarChar)},

                {typeof(DateTime), new ColumnInfo(SqlDbType.DateTime2)},
                {typeof(DateTimeOffset), new ColumnInfo(SqlDbType.DateTimeOffset)},

                {typeof(Guid), new ColumnInfo(SqlDbType.UniqueIdentifier)},
            };

            var debaserMapperAttribute = property.GetCustomAttribute<DebaserMapperAttribute>();
            var debaserTypeAttribute = property.GetCustomAttribute<DebaserSqlTypeAttribute>();

            if (debaserTypeAttribute != null && debaserMapperAttribute != null)
            {
                throw new InvalidOperationException($@"Found both [DebaserSqlType(...)] and [DebaserMapper(...)] on property {property.Name} of {property.DeclaringType}, but it's only possible to use one of them.

Please use [DebaserSqlType(...)] if you simply want to

* specify VARCHAR or NVARCHAR
* specify length of VARCNAR/NVARCHAR
* explicitly coerce into SQL types like [Date], [DateTime], etc.

Please use [DebaserMapper(...)] if you want to 

* have actual mapping between e.g. your domain's value types, like PostalCode, Currency, Money, etc., or
* map back and forth between value domains (could e.g. encrypt data at rest or serialize to/from JSON), or
* do whatever you like");
            }

            if (debaserTypeAttribute != null)
            {
                return new[]
                {
                    new ColumnInfo(
                        sqlDbType: debaserTypeAttribute.SqlDbType,
                        size: debaserTypeAttribute.Size,
                        addSize: debaserTypeAttribute.AltSize
                    )
                };
            }

            if (debaserMapperAttribute != null)
            {
                return GetColumInfoFromDebaserMapper(debaserMapperAttribute.DebaserMapperType);
            }

            if (defaultDbTypes.TryGetValue(property.PropertyType, out var columnInfo))
            {
                return new[] { columnInfo };
            }

            throw new ArgumentException($@"Could not automatically generate column info for {property}. Please use one of the types supported out-of-the-box:

{string.Join(Environment.NewLine, defaultDbTypes.Select(kvp => $"* {kvp.Key} => {kvp.Value.GetTypeDefinition()}"))}

or decorate the property with either

* [DebaserSqlType(...)] attribute (to specify database type for values that can be automatically converted/coerced), or
* [DebaserMapper(...)] attribute (to achieve more complex mapping and have even more control),

so Debaser can tell how a given value is to be saved.");
        }

        static IEnumerable<ColumnInfo> GetColumInfoFromDebaserMapper(Type type)
        {
            var interfaces = type.GetInterfaces();

            if (interfaces.Any(i => i == typeof(IDebaserMapper)))
            {
                var mapper = CreateInstance<IDebaserMapper>(type);

                return new[]{new ColumnInfo(
                    sqlDbType: mapper.SqlDbType,
                    size: mapper.SizeOrNull,
                    addSize: mapper.AdditionalSizeOrNull,
                    customToDatabase: mapper.ToDatabase,
                    customFromDatabase: mapper.FromDatabase
                )};
            }

            if (interfaces.Any(i => i == typeof(IDebaserMapper2)))
            {
                var mapper = CreateInstance<IDebaserMapper2>(type);

                var columnSpecs = mapper.GetColumnSpecs();

                return columnSpecs
                    .Select(column => new ColumnInfo(
                        column.SqlDbType,
                        column.SizeOrNull,
                        column.AdditionalSizeOrNull
                    ));

            }

            throw new NotImplementedException("Multi-column debaser mapper not implemented yet");

        }

        static TMapper CreateInstance<TMapper>(Type type)
        {
            try
            {
                var instance = Activator.CreateInstance(type);

                if (instance is TMapper mapper)
                {
                    return mapper;
                }

                throw new ArgumentException($"Mapper does not implement the required {typeof(TMapper)} interface");
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not create Debaser mapper {type}", exception);
            }
        }
    }
}