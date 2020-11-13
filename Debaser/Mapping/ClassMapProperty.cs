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
        readonly Func<object, object[]> _toDatabase;
        readonly Func<object, object> _fromDatabase;
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
        public ClassMapProperty(string propertyName, IEnumerable<ColumnInfo> columns, bool isKey, PropertyInfo property, Func<object, object[]> toDatabase, Func<object, object> fromDatabase)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _toDatabase = toDatabase ?? throw new ArgumentNullException(nameof(toDatabase));
            _fromDatabase = fromDatabase ?? throw new ArgumentNullException(nameof(fromDatabase));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
            ColumnNames = Columns.Select(c => c.ColumnName).ToArray();
            IsKey = isKey;
            _accessor = TypeAccessor.Create(property.DeclaringType);
        }

        /// <summary>
        /// Changes the property to be PK (or part of a composite PK) for the table
        /// </summary>
        public void MakeKey() => IsKey = true;

        /// <summary>
        /// Gets the necessary SQL to make out a single column definition line in a 'CREATE TABLE (...)' statement
        /// </summary>
        public string GetColumnDefinition() => string.Join(", ", Columns.Select(info => $"[{info.ColumnName}] {info.GetTypeDefinition()}"));

        internal void WriteTo(SqlDataRecord record, object row)
        {
            var value = _accessor[row, PropertyName];
            var valuesToWrite = _toDatabase(value);

            AssertLength(valuesToWrite);

            for (var index = 0; index < Columns.Count; index++)
            {
                var column = Columns[index];
                var toDatabase = column.ToDatabase;
                var valueToWrite = toDatabase(valuesToWrite[index]);

                var name = column.ColumnName;
                var ordinal = record.GetOrdinal(name);

                record.SetValue(ordinal, valueToWrite);
            }
        }

        internal object FromDatabase(object value)
        {
            var valueToSet = _fromDatabase(value);

            return valueToSet;
        }

        internal IEnumerable<SqlMetaData> GetSqlMetaData() => Columns.Select(column => column.GetSqlMetaData());

        void AssertLength(object[] valuesToWrite)
        {
            if (valuesToWrite.Length == Columns.Count) return;

            var valuesString = string.Join(", ", valuesToWrite);
            var columnsString = string.Join(", ", ColumnNames);
            throw new InvalidOperationException(
                $"Class map property for {PropertyName} returned {valuesToWrite.Length} values ({valuesString}), but {Columns.Count} were expected ({columnsString})");
        }

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