namespace Debaser.Core.Attributes;

/// <summary>
/// Attribute that can be applied to a property to make it PK (or be part of a composite PK)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserKeyAttribute : Attribute
{
}