using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

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
                    throw new ArgumentException($@"At least one key property needs to be specified");
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
            foreach (var property in type.GetProperties())
            {
                var name = property.Name;
                var columnInfo = GetColumnInfo(property);
                var columnName = property.Name;
                yield return new ClassMapProperty(name, columnInfo, columnName, string.Equals("Id", name, StringComparison.CurrentCultureIgnoreCase));
            }
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

            throw new ArgumentException($"Could not automatically generate column info for {property}");
        }
    }
}