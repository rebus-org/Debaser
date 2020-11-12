using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Debaser.Attributes;
using Microsoft.Data.SqlClient.Server;

namespace Debaser.Mapping
{
    /// <summary>
    /// Represents a mapping from a type to a table in the database
    /// </summary>
    public class ClassMap
    {
        readonly List<ClassMapProperty> _properties;

        /// <summary>
        /// Creates the map for the given <paramref name="type"/> containing the given list of <paramref name="properties"/> to be mapped
        /// </summary>
        public ClassMap(Type type, IEnumerable<ClassMapProperty> properties)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            _properties = properties?.ToList() ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// Gets the mapped type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the sequence of properties
        /// </summary>
        public IEnumerable<ClassMapProperty> Properties => _properties;

        /// <summary>
        /// Gets the <see cref="SqlMetaData"/> for each property
        /// </summary>
        public SqlMetaData[] GetSqlMetaData()
        {
            return Properties
                .SelectMany(p => p.GetSqlMetaData())
                .ToArray();
        }

        /// <summary>
        /// Gets any extra criteria required for a potential UPDATE to be carried out
        /// </summary>
        public string GetExtraCriteria()
        {
            var criteria = Type.GetCustomAttributes<DebaserUpdateCriteriaAttribute>()
                .Select(a => $" AND {a.UpdateCriteria}");

            return string.Concat(criteria);
        }
    }
}