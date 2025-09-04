namespace Debaser.Core.Internals.Values;

public interface IValueLookup
{
    object GetValue(string name, Type desiredType);
}