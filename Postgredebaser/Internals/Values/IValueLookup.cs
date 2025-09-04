namespace Postgredebaser.Internals.Values;

interface IValueLookup
{
    object GetValue(string name);
}