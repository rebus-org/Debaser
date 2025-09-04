// This class has been moved to Debaser.Core.Internals.Reflection
// This file provides backward compatibility

using Debaser.Core.Internals.Values;

namespace Debaser.Internals.Reflection;

internal class Activator : Core.Internals.Reflection.Activator
{
    public Activator(Type type, IEnumerable<string> includedProperties) : base(type, includedProperties)
    {
    }
}