using System;

namespace Debaser.Values
{
    public interface IValueLookup
    {
        object GetValue(string name, Type desiredType);
    }
}