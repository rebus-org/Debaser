using System.Data;

namespace Debaser.Internals;

/// <summary>
/// Maps .NET types to their SQL Server counterparts
/// </summary>
static class TypeMap
{
    public static readonly Dictionary<Type, SqlDbType> KnownTypes = new()
    {
        {typeof(string), SqlDbType.NVarChar },
        {typeof(bool), SqlDbType.Bit },
        {typeof(byte), SqlDbType.TinyInt },
        {typeof(short), SqlDbType.SmallInt },
        {typeof(int), SqlDbType.Int },
        {typeof(long), SqlDbType.BigInt },
        {typeof(float), SqlDbType.Real },
        {typeof(double), SqlDbType.Float },
        {typeof(decimal), SqlDbType.Decimal },
        {typeof(DateTime), SqlDbType.DateTime2 },
        {typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
        {typeof(Guid), SqlDbType.UniqueIdentifier },
        {typeof(DateOnly), SqlDbType.Date },
    };

}