using System.Data;

namespace Debaser
{
    public interface IDebaserMapper
    {
        SqlDbType SqlDbType { get; }
        int? SizeOrNull { get; }
        int? AdditionalSizeOrNull { get; }
    }
}