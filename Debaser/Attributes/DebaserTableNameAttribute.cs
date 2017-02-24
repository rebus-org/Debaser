using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Specifies the table name when applied to an upserted class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DebaserTableNameAttribute : Attribute
    {
        public string TableName { get; }

        public DebaserTableNameAttribute(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            TableName = tableName;
        }

    }
}