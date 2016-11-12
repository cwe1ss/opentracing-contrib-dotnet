// Based on https://github.com/serilog/serilog-sinks-periodicbatching

using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTracing.Tracer.BatchReporter.Internal
{
    public class PortableTimer : IDisposable
    {
        private readonly object _stateLock = new object();

        private readonly Func<CancellationToken, Task> _onTick;
        private readonly Action<Exception, string> _onError;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        readonly Timer _timer;

        private bool _running;
        private bool _disposed;

        public PortableTimer(Func<CancellationToken, Task> onTick, Action<Exception, string> onError = null)
        {
            if (onTick == null)
                throw new ArgumentNullException(nameof(onTick));

            _onTick = onTick;
            _onError = onError;

            _timer = new Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval));

            lock (_stateLock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(PortableTimer));

                _timer.Change(interval, Timeout.InfiniteTimeSpan);
            }
        }

        private async void OnTick()
        {
            try
            {
                lock (_stateLock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    // There's a little bit of raciness here, but it's needed to support the
                    // current API, which allows the tick handler to reenter and set the next interval.

                    if (_running)
                    {
                        Monitor.Wait(_stateLock);

                        if (_disposed)
                        {
                            return;
                        }
                    }

                    _running = true;
                }

                if (!_cancel.Token.IsCancellationRequested)
                {
                    await _onTick(_cancel.Token);
                }
            }
            catch (OperationCanceledException tcx)
            {
                _onError?.Invoke(tcx, "The timer was canceled during invocation: {0}");
            }
            finally
            {
                lock (_stateLock)
                {
                    _running = false;
                    Monitor.PulseAll(_stateLock);
                }
            }
        }

        public void Dispose()
        {
            _cancel.Cancel();

            lock (_stateLock)
            {
                if (_disposed)
                    return;

                while (_running)
                {
                    Monitor.Wait(_stateLock);
                }

                _timer.Dispose();

                _disposed = true;
            }
        }
    }
}