using NpgsqlTypes;

namespace Postgredebaser.Attributes;

/// <summary>
/// Configures which <see cref="NpgsqlDbType"/> to use for the given property, and optionally also which size(s) to use.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserNpgsqlTypeAttribute : Attribute
{
    public NpgsqlDbType NpgsqlDbType { get; }
    public int? Size { get; }
    public int? AltSize { get; }

    public DebaserNpgsqlTypeAttribute(NpgsqlDbType npgsqlDbType)
    {
        NpgsqlDbType = npgsqlDbType;
    }

    public DebaserNpgsqlTypeAttribute(NpgsqlDbType npgsqlDbType, int size)
    {
        NpgsqlDbType = npgsqlDbType;
        Size = size;
    }

    public DebaserNpgsqlTypeAttribute(NpgsqlDbType npgsqlDbType, int size, int altSize)
    {
        NpgsqlDbType = npgsqlDbType;
        Size = size;
        AltSize = altSize;
    }
}