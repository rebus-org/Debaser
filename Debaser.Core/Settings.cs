using System.Data;

namespace Debaser.Core;

public class Settings(
    int commandTimeoutSeconds = 120,
    IsolationLevel transactionIsolationLevel = IsolationLevel.ReadCommitted)
{
    public int CommandTimeoutSeconds { get; } = commandTimeoutSeconds;
    public IsolationLevel TransactionIsolationLevel { get; } = transactionIsolationLevel;
}