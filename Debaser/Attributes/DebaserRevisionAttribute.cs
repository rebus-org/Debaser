using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Specify that the property (which must be an integer type, either <see cref="short"/>, <see cref="int"/>, or <see cref="long"/>)
    /// is used to indicate the revision of the row, which is then assumed to be stored in an append-only fashion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserRevisionAttribute : Attribute
    {
    }
}