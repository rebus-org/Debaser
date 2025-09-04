namespace Debaser.Core.Attributes;

/// <summary>
/// Attribute that can be applied to a property to affect how the value is roundtripped to/from a database column
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserMapperAttribute : Attribute
{
    public Type DebaserMapperType { get; }

    /// <summary>
    /// Contructs the attribute with the <paramref name="debaserMapperType"/> type as the roundtripper. The type must implement
    /// <see cref="IDebaserMapper{TDbType}"/> and have a default constructor
    /// </summary>
    public DebaserMapperAttribute(Type debaserMapperType)
    {
        DebaserMapperType = debaserMapperType ?? throw new ArgumentNullException(nameof(debaserMapperType));
    }
}