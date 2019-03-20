using System;

namespace Debaser.Internals.Values
{
    public interface IValueLookup
    {
        object GetValue(string name, Type desiredType);
    }
}