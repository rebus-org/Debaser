using System.Data;

namespace Debaser;

public class Settings(
    int commandTimeoutSeconds = 120,
    IsolationLevel transactionIsolationLevel = IsolationLevel.ReadCommitted)
{
    public int CommandTimeoutSeconds { get; } = commandTimeoutSeconds;
    public IsolationLevel TransactionIsolationLevel { get; } = transactionIsolationLevel;
}