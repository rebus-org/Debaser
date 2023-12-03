using System;

namespace Debaser.Attributes;

/// <summary>
/// Attribute that can be added to a property to indicate a criteria which must be fulfilled for an existing row to be overwritten by the row provided when calling the upsert helper.
/// The criteria MUST be a predicate that operates on two rows with a schema that matches the target table schema using the <code>[S]</code> and <code>[T]</code> placeholders, meaning
/// "source" and "target" respectively.
/// For example, the criteria <code>[S].[Rev] > [T].[Rev]</code> can be used to make an update conditional, depending on whether the <code>[Rev]</code> column of the source row
/// is greater than that of the target row
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DebaserUpdateCriteriaAttribute : Attribute
{
    public DebaserUpdateCriteriaAttribute(string updateCriteria)
    {
        if (updateCriteria == null) throw new ArgumentNullException(nameof(updateCriteria));
        UpdateCriteria = updateCriteria;
    }

    public string UpdateCriteria { get; }
}