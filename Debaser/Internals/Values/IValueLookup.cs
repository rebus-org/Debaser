using System;

namespace Debaser.Internals.Values
{
    interface IValueLookup
    {
        object GetValue(string name, Type desiredType);
    }
}