using Debaser.Core;
using Debaser.Core.Attributes;
using NpgsqlTypes;

namespace Postgredebaser;

/// <summary>
/// Interface of a custom value mapper for PostgreSQL. Implement this interface in a class and indicate that you want to
/// use the implemented class by applying the <see cref="DebaserMapperAttribute"/> to a property
/// </summary>
public interface IDebaserMapper : IDebaserMapper<NpgsqlDbType>
{
    /// <summary>
    /// Must return the column type for the column in the database
    /// </summary>
    NpgsqlDbType NpgsqlDbType { get; }

    /// <summary>
    /// Implementation of the generic DbType property
    /// </summary>
    NpgsqlDbType IDebaserMapper<NpgsqlDbType>.DbType => NpgsqlDbType;
}