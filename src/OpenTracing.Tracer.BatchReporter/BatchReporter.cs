// Based on https://github.com/serilog/serilog-sinks-periodicbatching

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using OpenTracing.Tracer.BatchReporter.Internal;

namespace OpenTracing.Tracer.BatchReporter
{
    /// <summary>
    /// <para>Base class for reporters that send spans in batches.
    /// Batching is triggered asynchronously on a timer.</para>
    /// <para>To avoid unbounded memory growth, old spans are discarded when the
    /// queue reaches its maximum size. Also, spans are dropped after too many failed attempts.</para>
    /// </summary>
    /// <remarks>
    /// During normal operation the timer will simply use the configured flush interval.
    /// When availabilty fluctuates, the class tracks the number of failed attempts, each time
    /// increasing the interval before reconnection is attempted (up to a set maximum) and at predefined
    /// points indicating that either the current batch, or entire waiting queue, should be dropped. This
    /// serves two purposes - first, a loaded receiver may need a temporary reduction in traffic while coming
    /// back online. Second, the sender needs to account for both bad batches (the first fault response) and
    /// also overproduction (the second, queue-dropping response). In combination these should provide a
    /// reasonable delivery effort but ultimately protect the sender from memory exhaustion.
    /// </remarks>
    public abstract class BatchReporterBase : IDisposable
    {
        public const string DropReasonShutdown = "Shutdown";
        public const string DropReasonQueueSizeExceeded = "MaxQueueSizeExceeded";
        public const string DropReasonBatchDropped = "BatchDropped";
        public const string DropReasonQueueDropped = "QueueDropped";

        private readonly BatchReporterOptions _options;
        private readonly ConcurrentQueue<ISpan> _queue = new ConcurrentQueue<ISpan>();
        private readonly List<ISpan> _waitingBatch = new List<ISpan>();
        private readonly PortableTimer _timer;
        private readonly TimerStatus _status;

        private readonly object _stateLock = new object();

        private bool _unloading;
        private bool _started;

        protected BatchReporterBase(BatchReporterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            _options = options;
            _status = new TimerStatus(options.FlushInterval, options.MinBackoffPeriod, options.MaxBackoffPeriod);
            _timer = new PortableTimer(_ => OnTick(), OnError);
        }

        /// <summary>
        /// Queues the span. If the reporter is being disposed, then the span is ignored.
        /// </summary>
        public void Report(ISpan span)
        {
            if (span == null)
                return;

            if (_unloading)
            {
                OnSpanDropped(1, DropReasonShutdown);
                return;
            }

            _queue.Enqueue(span);

            RemoveOldEntriesIfNecessary();

            if (!_started)
            {
                lock (_stateLock)
                {
                    if (_started || _unloading)
                        return;

                    _started = true;
                    _timer.Start(TimeSpan.Zero);
                }
            }
        }

        /// <summary>
        /// Report a batch of spans.
        /// </summary>
        protected abstract Task ReportBatchAsync(IReadOnlyCollection<ISpan> spans);

        /// <summary>
        /// The reporter catches exceptions from <see cref="ReportBatchAsync"/> to handle retry-logic etc.
        /// This method allows derived reporters to log them.
        /// </summary>
        protected virtual void OnError(Exception exception, string message)
        {
        }

        /// <summary>
        /// Allows derived classes to update metrics or log dropped spans.
        /// </summary>
        protected virtual void OnSpanDropped(int amount, string reason)
        {
        }

        private void RemoveOldEntriesIfNecessary()
        {
            // This doesn't use locking between Count and Dequeue,
            // so the queue could dequeue too many entries.
            // However, reaching the limit means that something is broken and it's
            // quite likely that more spans will have to be dropped anyway.
            // So it doesn't really matter if this isn't 100% accurate.
            while (_queue.Count >= _options.MaxQueueSize)
            {
                ISpan dropped;
                _queue.TryDequeue(out dropped);
                OnSpanDropped(1, DropReasonQueueSizeExceeded);
            }
        }

        private async Task OnTick()
        {
            try
            {
                bool batchWasFull;
                do
                {
                    ISpan next;
                    while (_waitingBatch.Count < _options.MaxBatchSize && _queue.TryDequeue(out next))
                    {
                        _waitingBatch.Add(next);
                    }

                    if (_waitingBatch.Count == 0)
                        return;

                    await ReportBatchAsync(_waitingBatch);

                    batchWasFull = _waitingBatch.Count >= _options.MaxBatchSize;
                    _waitingBatch.Clear();
                    _status.MarkSuccess();
                }
                while (batchWasFull); // Otherwise, allow the period to elapse
            }
            catch (Exception ex)
            {
                OnError(ex, "OnTick");
                _status.MarkFailure();
            }
            finally
            {
                if (_status.RecentFailures >= _options.FailuresBeforeDroppingBatch)
                {
                    OnSpanDropped(_waitingBatch.Count, "BatchDropped");
                    _waitingBatch.Clear();
                }

                if (_status.RecentFailures >= _options.FailuresBeforeDroppingQueue)
                {
                    ISpan span;
                    while (_queue.TryDequeue(out span))
                    {
                        OnSpanDropped(1, "QueueDropped");
                    }
                }

                lock (_stateLock)
                {
                    if (!_unloading)
                    {
                        _timer.Start(_status.NextInterval);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Free resources held by the reporter.
        /// </summary>
        /// <param name="disposing">If true, called because the object is being disposed; if false,
        /// the object is being disposed from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            CloseAndFlush();
        }

        private void CloseAndFlush()
        {
            lock (_stateLock)
            {
                if (!_started || _unloading)
                    return;

                _unloading = true;
            }

            _timer.Dispose();

            // This is the place where SynchronizationContext.Current is unknown and can be != null
            // so we prevent possible deadlocks here for sync-over-async downstream implementations
            ResetSyncContextAndWait(OnTick);
        }

        private void ResetSyncContextAndWait(Func<Task> taskFactory)
        {
            var prevContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                taskFactory().Wait();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevContext);
            }
        }
    }
}
