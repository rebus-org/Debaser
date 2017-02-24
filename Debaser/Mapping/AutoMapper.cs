using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Debaser.Attributes;
// ReSharper disable ArgumentsStyleLiteral

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
            try
            {
                var properties = GetProperties(type).ToList();

                if (!properties.Any(p => p.IsKey))
                {
                    throw new ArgumentException(@"At least one key property needs to be specified. Either create a property named 'Id' or apply [DebaserKey] on one or more properties");
                }

                var revisionProperties = properties.Where(p => p.IsRevision).ToList();

                if (revisionProperties.Count > 1)
                {
                    throw new ArgumentException($@"At most one property can be a revision property (i.e. be decorated with the [DebaserRevision] attribute, but the following properties currently have it: {string.Join(", ", revisionProperties.Select(p => p.PropertyName))}");
                }

                if (revisionProperties.Count == 1)
                {
                    var revisionProperty = revisionProperties.Single();

                    var sqlDbType = revisionProperty.GetSqlMetaData().SqlDbType;

                    switch (sqlDbType)
                    {
                        default:
                            throw new ArgumentOutOfRangeException($"Revision property {revisionProperty.PropertyName} has SQL DB type {sqlDbType} - it must be either INT or BIGINT to work!");
                        case SqlDbType.Int:
                        case SqlDbType.BigInt:
                            break;
                    }
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
                .Where(property => !property.GetCustomAttributes<DebaserIgnoreAttribute>().Any())
                .Select(property =>
                {
                    var propertyName = property.Name;
                    var columnInfo = GetColumnInfo(property);
                    var columnName = GetColumnNameFromAttribute(property) ?? property.Name;
                    var isKey = property.GetCustomAttributes<DebaserKeyAttribute>().Any();

                    var toDatabase = columnInfo.CustomToDatabase ?? DefaultToDatabase();
                    var fromDatabase = columnInfo.CustomFromDatabase ?? DefaultFromDatabase();

                    var isRevision = property.GetCustomAttributes<DebaserRevisionAttribute>().Any();

                    return new ClassMapProperty(propertyName, columnInfo, columnName, isKey, toDatabase, fromDatabase, property, isRevision);
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

        static string GetColumnNameFromAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute<DebaserColumnNameAttribute>()?.ColumnName;
        }

        static Func<object, object> DefaultFromDatabase()
        {
            return obj => obj;
        }

        static Func<object, object> DefaultToDatabase()
        {
            return obj => obj;
        }

        static ColumnInfo GetColumnInfo(PropertyInfo property)
        {
            var defaultDbTypes = new Dictionary<Type, ColumnInfo>
            {
                {typeof(bool), new ColumnInfo(SqlDbType.Bit)},
                {typeof(bool?), new ColumnInfo(SqlDbType.Bit)},
                {typeof(byte), new ColumnInfo(SqlDbType.TinyInt)},
                {typeof(byte?), new ColumnInfo(SqlDbType.TinyInt)},
                {typeof(short), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(short?), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(int), new ColumnInfo(SqlDbType.Int)},
                {typeof(int?), new ColumnInfo(SqlDbType.Int)},
                {typeof(long), new ColumnInfo(SqlDbType.BigInt)},
                {typeof(long?), new ColumnInfo(SqlDbType.BigInt)},

                {typeof(decimal), new ColumnInfo(SqlDbType.Decimal, size: 18, addSize: 5)},
                {typeof(decimal?), new ColumnInfo(SqlDbType.Decimal, size: 18, addSize: 5)},
                {typeof(double), new ColumnInfo(SqlDbType.Float)},
                {typeof(double?), new ColumnInfo(SqlDbType.Float)},
                {typeof(float), new ColumnInfo(SqlDbType.Real)},
                {typeof(float?), new ColumnInfo(SqlDbType.Real)},

                {typeof(string), new ColumnInfo(SqlDbType.NVarChar)},

                {typeof(DateTime), new ColumnInfo(SqlDbType.DateTime2)},
                {typeof(DateTimeOffset), new ColumnInfo(SqlDbType.DateTimeOffset)},

                {typeof(Guid), new ColumnInfo(SqlDbType.UniqueIdentifier)},
            };

            ColumnInfo columnInfo;

            if (defaultDbTypes.TryGetValue(property.PropertyType, out columnInfo))
                return columnInfo;

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