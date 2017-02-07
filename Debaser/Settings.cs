using System.Data;

namespace Debaser
{
    public class Settings
    {
        public Settings(int commandTimeoutSeconds = 120, IsolationLevel transactionIsolationLevel = IsolationLevel.ReadCommitted)
        {
            CommandTimeoutSeconds = commandTimeoutSeconds;
            TransactionIsolationLevel = transactionIsolationLevel;
        }

        public int CommandTimeoutSeconds { get; }
        public IsolationLevel TransactionIsolationLevel { get; }
    }
}