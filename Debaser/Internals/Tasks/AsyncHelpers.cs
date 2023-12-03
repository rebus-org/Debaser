﻿using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Debaser.Internals.Tasks;

static class AsyncHelpers
{
    public static TResult GetSync<TResult>(Func<Task<TResult>> task)
    {
        var result = default(TResult);
        RunSync(async () => { result = await task(); });
        return result;
    }

    /// <summary>
    /// Executes a task synchronously on the calling thread by installing a temporary synchronization context that queues continuations
    ///  </summary>
    public static void RunSync(Func<Task> task)
    {
        var currentContext = SynchronizationContext.Current;
        var customContext = new CustomSynchronizationContext(task);

        try
        {
            SynchronizationContext.SetSynchronizationContext(customContext);

            customContext.Run();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(currentContext);
        }
    }

    /// <summary>
    /// Synchronization context that can be "pumped" in order to have it execute continuations posted back to it
    /// </summary>
    class CustomSynchronizationContext : SynchronizationContext
    {
        readonly ConcurrentQueue<Tuple<SendOrPostCallback, object>> _items = new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();
        readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);
        readonly Func<Task> _task;

        ExceptionDispatchInfo _caughtException;

        bool _done;

        public CustomSynchronizationContext(Func<Task> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task), "Please remember to pass a Task to be executed");
        }

        public override void Post(SendOrPostCallback function, object state)
        {
            _items.Enqueue(Tuple.Create(function, state));
            _workItemsWaiting.Set();
        }

        /// <summary>
        /// Enqueues the function to be executed and executes all resulting continuations until it is completely done
        /// </summary>
        public void Run()
        {
            Post(async _ =>
            {
                try
                {
                    await _task();
                }
                catch (Exception exception)
                {
                    _caughtException = ExceptionDispatchInfo.Capture(exception);
                    throw;
                }
                finally
                {
                    Post(state => _done = true, null);
                }
            }, null);

            while (!_done)
            {
                if (_items.TryDequeue(out var task))
                {
                    task.Item1(task.Item2);

                    if (_caughtException == null) continue;

                    _caughtException.Throw();
                }
                else
                {
                    _workItemsWaiting.WaitOne();
                }
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("Cannot send to same thread");
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}