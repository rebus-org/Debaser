// This class now uses the shared Activator from Debaser.Core
// This provides better record support and consistency

using Debaser.Core.Internals.Reflection;

namespace Postgredebaser.Internals.Reflection;

internal class Activator : Debaser.Core.Internals.Reflection.Activator
{
    public Activator(Type type, IEnumerable<string> propertyNames) : base(type, propertyNames)
    {
    }
}