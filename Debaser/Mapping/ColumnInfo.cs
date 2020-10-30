using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient.Server;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Debaser.Mapping
{
    /// <summary>
    /// Represents a single column in the database
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Gets the default length of NVARCHAR columns
        /// </summary>
        public const int DefaultNVarcharLength = 256;

        /// <summary>
        /// Creates the column info
        /// </summary>
        public ColumnInfo(SqlDbType sqlDbType, int? size = null, int? addSize = null, Func<object, object> customToDatabase = null, Func<object, object> customFromDatabase = null)
        {
            SqlDbType = sqlDbType;
            Size = size;
            AddSize = addSize;
            ToDatabase = customToDatabase ?? DefaultToDatabase();
            FromDatabase = customFromDatabase ?? DefaultFromDatabase();
        }

        public string ColumnName { get; }

        /// <summary>
        /// Gets the SQL database column type
        /// </summary>
        public SqlDbType SqlDbType { get; }

        /// <summary>
        /// Gets the size (or null if irrelevant)
        /// </summary>
        public int? Size { get; }

        /// <summary>
        /// Gets the additional size (or null if irrelevant)
        /// Can be used to specify decimal places in the DECIMAL size specification
        /// </summary>
        public int? AddSize { get; }

        internal Func<object, object> ToDatabase { get; }

        internal Func<object, object> FromDatabase { get; }

        internal string GetTypeDefinition()
        {
            if (Size == null)
            {
                if (IsString)
                {
                    return $"{SqlDbType.ToString().ToUpper()}({DefaultNVarcharLength})";
                }
                return $"{SqlDbType.ToString().ToUpper()}";
            }

            if (Size != null && AddSize != null)
            {
                return $"{SqlDbType.ToString().ToUpper()}({Size.Value},{AddSize.Value})";
            }

            var sizeString = GetSizeString(Size.Value);

            return $"{SqlDbType.ToString().ToUpper()}({sizeString})";
        }

        string GetSizeString(int size)
        {
            return SizeIsMax(size) ? "MAX" : size.ToString();
        }

        internal SqlMetaData GetSqlMetaData(string columnName)
        {
            if (!Size.HasValue)
            {
                if (IsString)
                {
                    return new SqlMetaData(columnName, SqlDbType, maxLength: DefaultNVarcharLength);
                }

                return new SqlMetaData(columnName, SqlDbType);
            }

            if (SizeIsMax(Size.Value) && IsString)
            {
                return new SqlMetaData(columnName, SqlDbType, maxLength: SqlMetaData.Max);
            }

            if (Size != null && AddSize != null)
            {
                return new SqlMetaData(columnName, SqlDbType, precision: (byte)Size.Value, scale: (byte)AddSize.Value);
            }

            return new SqlMetaData(columnName, SqlDbType, maxLength: Size.Value);
        }

        static bool SizeIsMax(int size) => size == int.MaxValue;

        bool IsString => new[] { SqlDbType.NVarChar, SqlDbType.VarChar }.Contains(SqlDbType);

        static Func<object, object> DefaultFromDatabase() => obj => obj == DBNull.Value ? null : obj;

        static Func<object, object> DefaultToDatabase() => obj => obj;
    }
}