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
        public string Name { get; }
        public ColumnInfo ColumnInfo { get; }

        public ClassMapProperty(string name, ColumnInfo columnInfo)
        {
            Name = name;
            ColumnInfo = columnInfo;
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
    }
}