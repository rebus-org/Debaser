// This class has been moved to Debaser.Core.Internals.Tasks
// This file provides backward compatibility

// ReSharper disable AsyncVoidLambda

using Debaser.Core.Internals.Tasks;

namespace Debaser.Internals.Tasks;

static class AsyncHelpers
{
    public static TResult GetSync<TResult>(Func<Task<TResult>> task) => Core.Internals.Tasks.AsyncHelpers.GetSync(task);
    public static void RunSync(Func<Task> task) => Core.Internals.Tasks.AsyncHelpers.RunSync(task);
}