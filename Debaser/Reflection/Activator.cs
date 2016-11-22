using System;
using Debaser.Values;

namespace Debaser.Reflection
{
    public class Activator
    {
        public object CreateInstance(Type type, IValueLookup valueLookup)
        {
            return new object();
        }
    }
}