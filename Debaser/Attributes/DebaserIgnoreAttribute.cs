// This class has been moved to Debaser.Core.Attributes
// This file provides backward compatibility

using Debaser.Core.Attributes;

namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be applied to a property to make Debaser ignore it
/// </summary>
[Obsolete("Use Debaser.Core.Attributes.DebaserIgnoreAttribute instead")]
public class DebaserIgnoreAttribute : Core.Attributes.DebaserIgnoreAttribute
{
}