using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Attribute that can be appliued to a property to make it PK (or be part of a composite PK)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserKeyAttribute : Attribute
    {

    }
}