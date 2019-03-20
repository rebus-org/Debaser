using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Debaser.Attributes;

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
            if (type == null) throw new ArgumentNullException(nameof(type));

            Func<object, object> DefaultFromDatabase() => obj => obj;

            Func<object, object> DefaultToDatabase() => obj => obj;

            var properties = type.GetProperties()
                .Where(property => !property.GetCustomAttributes<DebaserIgnoreAttribute>().Any())
                .Select(property =>
                {
                    var propertyName = property.Name;
                    var columnInfo = GetColumnInfo(property);
                    var columnName = property.Name;
                    var isKey = property.GetCustomAttributes<DebaserKeyAttribute>().Any();

                    var toDatabase = columnInfo.CustomToDatabase ?? DefaultToDatabase();
                    var fromDatabase = columnInfo.CustomFromDatabase ?? DefaultFromDatabase();

                    return new ClassMapProperty(propertyName, columnInfo, columnName, isKey, toDatabase, fromDatabase, property);
                })
                .ToList();

            if (!properties.Any(p => p.IsKey))
            {
                var defaultKeyProperty = properties.FirstOrDefault(p => string.Equals("Id", p.PropertyName));

                if (defaultKeyProperty == null)
                {
                    throw new ArgumentException("Could not find property named 'Id'");
                }

                defaultKeyProperty.MakeKey();
            }

            return properties;
        }

        static ColumnInfo GetColumnInfo(PropertyInfo property)
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

            if (defaultDbTypes.TryGetValue(property.PropertyType, out var columnInfo))
            {
                return columnInfo;
            }

            var debaserMapperAttribute = property.GetCustomAttribute<DebaserMapperAttribute>();

            if (debaserMapperAttribute != null)
            {
                return GetColumInfoFromDebaserMapper(debaserMapperAttribute.DebaserMapperType);
            }

            throw new ArgumentException($"Could not automatically generate column info for {property}");
        }

        static ColumnInfo GetColumInfoFromDebaserMapper(Type type)
        {
            var mapper = CreateInstance(type);

            return new ColumnInfo(mapper.SqlDbType, mapper.SizeOrNull, mapper.AdditionalSizeOrNull, mapper.ToDatabase, mapper.FromDatabase);
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