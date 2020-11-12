using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastMember;
using Microsoft.Data.SqlClient.Server;

namespace Debaser.Mapping
{
    /// <summary>
    /// Represents a single mapped property
    /// </summary>
    public class ClassMapProperty
    {
        readonly TypeAccessor _accessor;

        /// <summary>
        /// Gets the name of the database column
        /// </summary>
        public IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        /// Gets the name of the property
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the columns to which this propert should be mapped
        /// </summary>
        public IReadOnlyList<ColumnInfo> Columns { get; }

        /// <summary>
        /// Gets whether the property is to be PK (or part of a composite PK) for the table
        /// </summary>
        public bool IsKey { get; private set; }

        /// <summary>
        /// Creates the property
        /// </summary>
        public ClassMapProperty(string propertyName, IEnumerable<ColumnInfo> columns, bool isKey, PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
            ColumnNames = Columns.Select(c => c.ColumnName).ToArray();
            IsKey = isKey;
            _accessor = TypeAccessor.Create(property.DeclaringType);
        }

        /// <summary>
        /// Changes the property to be PK (or part of a composite PK) for the table
        /// </summary>
        public void MakeKey()
        {
            IsKey = true;
        }

        /// <summary>
        /// Gets the necessary SQL to make out a single column definition line in a 'CREATE TABLE (...)' statement
        /// </summary>
        public string GetColumnDefinition()
        {
            return string.Join(", ", Columns.Select(info => $"[{info.ColumnName}] {info.GetTypeDefinition()}"));
        }

        internal void WriteTo(SqlDataRecord record, object row)
        {
            foreach (var column in Columns)
            {
                var name = column.ColumnName;
                var toDatabase = column.ToDatabase;
                var ordinal = record.GetOrdinal(name);
                var value = _accessor[row, PropertyName];
                var valueToWrite = toDatabase(value);

                record.SetValue(ordinal, valueToWrite);
            }
        }

        internal object FromDatabase(object value)
        {
            var valueToSet = _fromDatabase(value);

            return valueToSet;
        }

        object ToDatabase(object target)
        {
            return _toDatabase(target);
        }

        internal IEnumerable<SqlMetaData> GetSqlMetaData() => Columns.Select(column => column.GetSqlMetaData());

        /// <summary>
        /// Gets a nice string that represents the property
        /// </summary>
        public override string ToString()
        {
            var columnNamesString = string.Join(", ", ColumnNames.Select(columnName => $"[{columnName}]"));

            return IsKey
                ? $"{PropertyName} ({columnNamesString} PK)"
                : $"{PropertyName} ({columnNamesString})";
        }
    }
}