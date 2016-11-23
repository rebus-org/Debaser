using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Server;

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

        public SqlMetaData[] GetSqlMetaData()
        {
            return Properties
                .Select(p => p.GetSqlMetaData())
                .ToArray();
        }
    }
}