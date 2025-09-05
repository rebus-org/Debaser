// ReSharper disable CheckNamespace
using Debaser.Core;

namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be applied to a property to make Debaser ignore it
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserIgnoreAttribute : Attribute;

/// <summary>
/// Attribute that can be applied to a property to make it PK (or be part of a composite PK)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DebaserKeyAttribute : Attribute;

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

/// <summary>
/// Include as an update criteria the requirement that this particular property has a value that is incremented compared to the previous value
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DebaserRevisionCriteriaAttribute(string propertyName) : DebaserUpdateCriteriaAttribute($"[S].[{propertyName}] > [T].[{propertyName}]");

/// <summary>
/// Attribute that can be added to a property to indicate a criteria which must be fulfilled for an existing row to be overwritten by the row provided when calling the upsert helper.
/// The criteria MUST be a predicate that operates on two rows with a schema that matches the target table schema using the <code>[S]</code> and <code>[T]</code> placeholders, meaning
/// "source" and "target" respectively.
/// For example, the criteria <code>[S].[Rev] > [T].[Rev]</code> can be used to make an update conditional, depending on whether the <code>[Rev]</code> column of the source row
/// is greater than that of the target row
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DebaserUpdateCriteriaAttribute(string updateCriteria) : Attribute
{
    public string UpdateCriteria { get; } = updateCriteria ?? throw new ArgumentNullException(nameof(updateCriteria));
}