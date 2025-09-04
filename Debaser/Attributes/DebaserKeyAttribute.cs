// This class has been moved to Debaser.Core.Attributes
// This file provides backward compatibility

using Debaser.Core.Attributes;

namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be applied to a property to make it PK (or be part of a composite PK)
/// </summary>
[Obsolete("Use Debaser.Core.Attributes.DebaserKeyAttribute instead")]
public class DebaserKeyAttribute : Core.Attributes.DebaserKeyAttribute
{
}