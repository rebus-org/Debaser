// This class has been moved to Debaser.Core.Attributes
// This file provides backward compatibility

using Debaser.Core.Attributes;

namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be applied to a property to affect how the value is roundtripped to/from a database column
/// </summary>
[Obsolete("Use Debaser.Core.Attributes.DebaserMapperAttribute instead")]
public class DebaserMapperAttribute : Core.Attributes.DebaserMapperAttribute
{
    /// <summary>
    /// Contructs the attribute with the <paramref name="debaserMapperType"/> type as the roundtripper. The type must implement
    /// <see cref="IDebaserMapper"/> and have a default constructor
    /// </summary>
    public DebaserMapperAttribute(Type debaserMapperType) : base(debaserMapperType)
    {
    }
}