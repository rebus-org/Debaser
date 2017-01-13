using System;
using System.Reflection;
using FastMember;
using Microsoft.SqlServer.Server;

namespace Debaser.Mapping
{
    public class ClassMapProperty
    {
        readonly Func<object, object> _toDatabase;
        readonly Func<object, object> _fromDatabase;
        readonly TypeAccessor _accessor;
        readonly string _propertyName;

        public string ColumnName { get; }
        public string Name { get; }
        public ColumnInfo ColumnInfo { get; }
        public bool IsKey { get; private set; }

        public ClassMapProperty(string name, ColumnInfo columnInfo, string columnName, bool isKey, Func<object, object> toDatabase, Func<object, object> fromDatabase, PropertyInfo property)
        {
            Name = name;
            ColumnInfo = columnInfo;
            ColumnName = columnName;
            IsKey = isKey;
            _toDatabase = toDatabase;
            _fromDatabase = fromDatabase;
            _accessor = TypeAccessor.Create(property.DeclaringType);
            _propertyName = property.Name;
        }

        public void MakeKey()
        {
            IsKey = true;
        }

        public object ToDatabase(object target)
        {
            return _toDatabase(target);
        }

        public string GetColumnDefinition()
        {
            return $"[{ColumnName}] {ColumnInfo.GetTypeDefinition()}";
        }

        public void WriteTo(SqlDataRecord record, object row)
        {
            var ordinal = record.GetOrdinal(ColumnName);
            var value = _accessor[row, _propertyName];
            var valueToWrite = ToDatabase(value);

            record.SetValue(ordinal, valueToWrite);
        }

        public object FromDatabase(object value)
        {
            var valueToSet = _fromDatabase(value);

            return valueToSet;
        }

        public SqlMetaData GetSqlMetaData()
        {
            return ColumnInfo.GetSqlMetaData(ColumnName);
        }

        public override string ToString()
        {
            return IsKey
                ? $"{Name} ([{ColumnName}] PK)"
                : $"{Name} ([{ColumnName}])";
        }
    }
}