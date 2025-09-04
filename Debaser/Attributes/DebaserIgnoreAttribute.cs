namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be applied to a property to make Debaser ignore it
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserIgnoreAttribute : Attribute
{
}