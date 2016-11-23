using System;
using Microsoft.SqlServer.Server;

namespace Debaser.Mapping
{
    public class ClassMapProperty
    {
        readonly Func<object, object> _getValue;
        readonly Action<object, object> _setValue;

        public string ColumnName { get; }
        public string Name { get; }
        public ColumnInfo ColumnInfo { get; }
        public bool IsKey { get; }

        public ClassMapProperty(string name, ColumnInfo columnInfo, string columnName, bool isKey, Func<object, object> getValue, Action<object, object> setValue)
        {
            Name = name;
            ColumnInfo = columnInfo;
            ColumnName = columnName;
            IsKey = isKey;
            _getValue = getValue;
            _setValue = setValue;
        }

        public object GetValue(object target)
        {
            return _getValue(target);
        }

        public void SetValue(object target, object value)
        {
            _setValue(target, value);
        }

        public string GetColumnDefinition()
        {
            return $"[{ColumnName}] {ColumnInfo.GetTypeDefinition()}";
        }

        public void WriteTo(SqlDataRecord record, object row)
        {
            var ordinal = record.GetOrdinal(ColumnName);
            var value = GetValue(row);

            record.SetValue(ordinal, value);
            return;
            if (value == null)
            {
                record.SetDBNull(ordinal);
            }
            else
            {
            }
        }

        public SqlMetaData GetSqlMetaData()
        {
            return ColumnInfo.GetSqlMetaData(ColumnName);
        }
    }
}