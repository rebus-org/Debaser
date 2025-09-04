// This interface now inherits from Debaser.Core.Internals.Values.IValueLookup
// We provide a wrapper for the old single-parameter GetValue method

using Debaser.Core.Internals.Values;

namespace Postgredebaser.Internals.Values;

interface IValueLookup : Debaser.Core.Internals.Values.IValueLookup
{
    // Keep the old method for backward compatibility in our own code
    object GetValue(string name);
}