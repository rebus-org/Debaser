using System.Data;
using System.Linq;
using Microsoft.SqlServer.Server;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Debaser.Mapping
{
    public class ColumnInfo
    {
        public const int DefaultNVarcharLength = 256;

        public SqlDbType SqlDbType { get; }
        public int? Size { get; }
        public int? AddSize { get; }

        public ColumnInfo(SqlDbType sqlDbType, int? size = null, int? addSize = null)
        {
            SqlDbType = sqlDbType;
            Size = size;
            AddSize = addSize;
        }

        public string GetTypeDefinition()
        {
            if (Size == null)
            {
                if (IsString)
                {
                    return $"{SqlDbType.ToString().ToUpper()}({DefaultNVarcharLength})";
                }
                return $"{SqlDbType.ToString().ToUpper()}";
            }

            var sizeString = GetSizeString(Size.Value);

            return $"{SqlDbType.ToString().ToUpper()}({sizeString})";
        }

        string GetSizeString(int size)
        {
            return SizeIsMax(size) ? "MAX" : size.ToString();
        }

        public SqlMetaData GetSqlMetaData(string columnName)
        {
            if (!Size.HasValue || SizeIsMax(Size.Value))
            {
                if (IsString)
                {
                    return new SqlMetaData(columnName, SqlDbType, maxLength: DefaultNVarcharLength);
                }

                return new SqlMetaData(columnName, SqlDbType);
            }

            return new SqlMetaData(columnName, SqlDbType, maxLength: Size.Value);
        }

        static bool SizeIsMax(int size)
        {
            return size == int.MaxValue;
        }

        bool IsString => new[] { SqlDbType.NVarChar, SqlDbType.VarChar }.Contains(SqlDbType);
    }
}