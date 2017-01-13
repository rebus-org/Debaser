using System;
using System.Reflection;
using FastMember;
using Microsoft.SqlServer.Server;

namespace Debaser.Mapping
{
    /// <summary>
    /// Represents a single mapped property
    /// </summary>
    public class ClassMapProperty
    {
        readonly Func<object, object> _toDatabase;
        readonly Func<object, object> _fromDatabase;
        readonly TypeAccessor _accessor;

        /// <summary>
        /// Gets the name of the database column
        /// </summary>
        public string ColumnName { get; }
        
        /// <summary>
        /// Gets the name of the property
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the database column information for this property
        /// </summary>
        public ColumnInfo ColumnInfo { get; }

        /// <summary>
        /// Gets whether the property is to be PK (or part of a composite PK) for the table
        /// </summary>
        public bool IsKey { get; private set; }

        /// <summary>
        /// Creates the property
        /// </summary>
        public ClassMapProperty(string propertyName, ColumnInfo columnInfo, string columnName, bool isKey, Func<object, object> toDatabase, Func<object, object> fromDatabase, PropertyInfo property)
        {
            PropertyName = propertyName;
            ColumnInfo = columnInfo;
            ColumnName = columnName;
            IsKey = isKey;
            _toDatabase = toDatabase;
            _fromDatabase = fromDatabase;
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
            return $"[{ColumnName}] {ColumnInfo.GetTypeDefinition()}";
        }

        internal void WriteTo(SqlDataRecord record, object row)
        {
            var ordinal = record.GetOrdinal(ColumnName);
            var value = _accessor[row, PropertyName];
            var valueToWrite = ToDatabase(value);

            record.SetValue(ordinal, valueToWrite);
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

        internal SqlMetaData GetSqlMetaData()
        {
            return ColumnInfo.GetSqlMetaData(ColumnName);
        }

        /// <summary>
        /// Gets a nice string that represents the property
        /// </summary>
        public override string ToString()
        {
            return IsKey
                ? $"{PropertyName} ([{ColumnName}] PK)"
                : $"{PropertyName} ([{ColumnName}])";
        }
    }
}