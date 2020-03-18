using System;
using System.Data;
// ReSharper disable UnusedMember.Global

namespace Debaser.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserSqlTypeAttribute : Attribute
    {
        public SqlDbType SqlDbType { get; }
        public int? Size { get; }
        public int? AltSize { get; }

        public DebaserSqlTypeAttribute(SqlDbType sqlDbType)
        {
            SqlDbType = sqlDbType;
        }

        public DebaserSqlTypeAttribute(SqlDbType sqlDbType, int size)
        {
            SqlDbType = sqlDbType;
            Size = size;
        }

        public DebaserSqlTypeAttribute(SqlDbType sqlDbType, int size, int altSize)
        {
            SqlDbType = sqlDbType;
            Size = size;
            AltSize = altSize;
        }
    }
}