using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Debaser.Attributes;

namespace Debaser.Mapping
{
    public class AutoMapper
    {
        public ClassMap GetMap(Type type)
        {
            try
            {
                var properties = GetProperties(type).ToList();

                if (!properties.Any(p => p.IsKey))
                {
                    throw new ArgumentException(@"At least one key property needs to be specified. Either create a property named 'Id' or apply [DebaserKey] on one or more properties");
                }

                return new ClassMap(type, properties);
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Could not auto-map {type}", exception);
            }
        }

        static IEnumerable<ClassMapProperty> GetProperties(Type type)
        {
            var properties = type.GetProperties()
                .Select(property =>
                {
                    var name = property.Name;
                    var columnInfo = GetColumnInfo(property);
                    var columnName = property.Name;
                    var isKey = property.GetCustomAttributes<DebaserKeyAttribute>().Any();

                    return new ClassMapProperty(name, columnInfo, columnName, isKey, DefaultGetter(property),
                        DefaultSetter(property));
                })
                .ToList();

            if (!properties.Any(p => p.IsKey))
            {
                var defaultKeyProperty = properties.FirstOrDefault(p => string.Equals("Id", p.Name));

                if (defaultKeyProperty == null)
                {
                    throw new ArgumentException("Could not find property named 'Id'");
                }

                defaultKeyProperty.MakeKey();
            }

            return properties;
        }

        static Action<object, object> DefaultSetter(PropertyInfo property)
        {
            return property.SetValue;
        }

        static Func<object, object> DefaultGetter(PropertyInfo property)
        {
            return property.GetValue;
        }

        static ColumnInfo GetColumnInfo(PropertyInfo property)
        {
            var defaultDbTypes = new Dictionary<Type, ColumnInfo>
            {
                {typeof(short), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(int), new ColumnInfo(SqlDbType.Int)},
                {typeof(long), new ColumnInfo(SqlDbType.BigInt)},

                {typeof(decimal), new ColumnInfo(SqlDbType.Decimal)},
                {typeof(double), new ColumnInfo(SqlDbType.Decimal)},
                {typeof(float), new ColumnInfo(SqlDbType.Decimal)},

                {typeof(string), new ColumnInfo(SqlDbType.NVarChar)},

                {typeof(DateTime), new ColumnInfo(SqlDbType.DateTime2)},
                {typeof(DateTimeOffset), new ColumnInfo(SqlDbType.DateTimeOffset)},
            };

            ColumnInfo columnInfo;

            if (defaultDbTypes.TryGetValue(property.PropertyType, out columnInfo))
                return columnInfo;

            var debaserMapperAttribute = property.GetCustomAttribute<DebaserMapperAttribute>();

            if (debaserMapperAttribute != null)
            {
                return GetColumInfoFromDebaserMapper(debaserMapperAttribute.Type);
            }

            throw new ArgumentException($"Could not automatically generate column info for {property}");
        }

        static ColumnInfo GetColumInfoFromDebaserMapper(Type type)
        {
            var mapper = CreateInstance(type);

            return new ColumnInfo(mapper.SqlDbType, mapper.SizeOrNull, mapper.AdditionalSizeOrNull);
        }

        static IDebaserMapper CreateInstance(Type type)
        {
            try
            {
                return (IDebaserMapper)Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not create Debaser mapper {type}", exception);
            }
        }
    }
}