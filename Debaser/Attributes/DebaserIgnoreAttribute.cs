using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Attribute that can be appliued to a property to make Debaser ignore it
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserIgnoreAttribute : Attribute
    {
    }
}