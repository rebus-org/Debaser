using System;

namespace Debaser.Values
{
    public interface IValueLookup
    {
        object GetValue(object obj, string name, Type desiredType);
    }
}