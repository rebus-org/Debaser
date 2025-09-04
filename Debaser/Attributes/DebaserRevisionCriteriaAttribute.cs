// This class has been moved to Debaser.Core.Attributes
// This file provides backward compatibility

using Debaser.Core.Attributes;

namespace Debaser.Attributes;

/// <summary>
/// Include as an update criteria the requirement that this particular property has a value that is incremented compared to the previous value
/// </summary>
[Obsolete("Use Debaser.Core.Attributes.DebaserRevisionCriteriaAttribute instead")]
public class DebaserRevisionCriteriaAttribute(string propertyName) : Core.Attributes.DebaserRevisionCriteriaAttribute(propertyName);