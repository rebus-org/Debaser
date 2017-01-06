using System;

namespace Debaser.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DebaserMapperAttribute : Attribute
    {
        public Type Type { get; }

        public DebaserMapperAttribute(Type type)
        {
            Type = type;
        }
    }
}