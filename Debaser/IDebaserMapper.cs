using System.Data;
using Debaser.Core;
using Debaser.Core.Attributes;

namespace Debaser;

/// <summary>
/// Interface of a custom value mapper for SQL Server. Implement this interface in a class and indicate that you want to
/// use the implemented class by applying the <see cref="DebaserMapperAttribute"/> to a property
/// </summary>
public interface IDebaserMapper : IDebaserMapper<SqlDbType>
{
    /// <summary>
    /// Must return the column type for the column in the database
    /// </summary>
    SqlDbType SqlDbType { get; }

    /// <summary>
    /// Implementation of the generic DbType property
    /// </summary>
    SqlDbType IDebaserMapper<SqlDbType>.DbType => SqlDbType;
}