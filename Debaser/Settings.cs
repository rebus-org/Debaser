// This class has been moved to Debaser.Core
// This file provides backward compatibility

using System.Data;

namespace Debaser;

/// <summary>
/// SQL Server specific settings for Debaser
/// </summary>
[Obsolete("Use Debaser.Core.Settings instead")]
public class Settings : Core.Settings
{
    public Settings(int commandTimeoutSeconds = 120, IsolationLevel transactionIsolationLevel = IsolationLevel.ReadCommitted) 
        : base(commandTimeoutSeconds, transactionIsolationLevel)
    {
    }
}