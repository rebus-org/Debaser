using System;

namespace Debaser.Attributes
{
    /// <summary>
    /// Attribute that can be applied to a property to affect how the value is roundtripped to/from a database column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DebaserMapperAttribute : Attribute
    {
        internal Type DebaserMapperType { get; }

        /// <summary>
        /// Contructs the attribute with the <paramref name="debaserMapperType"/> type as the roundtripper. The type must implement
        /// <see cref="IDebaserMapper"/> and have a default constructor
        /// </summary>
        public DebaserMapperAttribute(Type debaserMapperType)
        {
            if (debaserMapperType == null) throw new ArgumentNullException(nameof(debaserMapperType));
            DebaserMapperType = debaserMapperType;
        }
    }
}