using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Specifies the column name when applied to an upserted class' properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserColumnNameAttribute : Attribute
    {
        public string ColumnName { get; }

        public DebaserColumnNameAttribute(string columnName)
        {
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));
            ColumnName = columnName;
        }

    }
}