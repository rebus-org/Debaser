using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Debaser.Mapping
{
    public class ClassMap
    {
        readonly List<ClassMapProperty> _properties;

        public ClassMap(Type type, IEnumerable<ClassMapProperty> properties)
        {
            Type = type;
            _properties = properties.ToList();
        }

        public Type Type { get; }

        public IEnumerable<ClassMapProperty> Properties => _properties;
    }

    public class ClassMapProperty
    {
        public string ColumnName { get; }
        public string Name { get; }
        public ColumnInfo ColumnInfo { get; }
        public bool IsKey { get; }

        public ClassMapProperty(string name, ColumnInfo columnInfo, string columnName, bool isKey)
        {
            Name = name;
            ColumnInfo = columnInfo;
            ColumnName = columnName;
            IsKey = isKey;
        }

        public string GetColumnDefinition()
        {
            return $"[{ColumnName}] {ColumnInfo.GetTypeDefinition()}";
        }
    }

    public class ColumnInfo
    {
        public SqlDbType SqlDbType { get; }
        public int? Size { get; }
        public int? AddSize { get; }

        public ColumnInfo(SqlDbType sqlDbType, int? size = null, int? addSize = null)
        {
            SqlDbType = sqlDbType;
            Size = size;
            AddSize = addSize;
        }

        public string GetTypeDefinition()
        {
            return Size == null
                ? $"{SqlDbType.ToString().ToUpper()}"
                : $"{SqlDbType.ToString().ToUpper()}({Size.Value})";
        }
    }
}