using System.Data;

namespace Debaser
{
    public interface IDebaserMapper
    {
        SqlDbType SqlDbType { get; }
        int? SizeOrNull { get; }
        int? AdditionalSizeOrNull { get; }
        object ToDatabase(object arg);
        object FromDatabase(object arg);
    }
}