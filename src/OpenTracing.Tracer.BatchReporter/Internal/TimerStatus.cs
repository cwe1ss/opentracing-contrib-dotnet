// Based on https://github.com/serilog/serilog-sinks-periodicbatching

using System;

namespace OpenTracing.Tracer.BatchReporter.Internal
{
    /// <summary>
    /// Manages the interval time during ticks. It uses exponential backoff in case of failures.
    /// </summary>
    public class TimerStatus
    {
        private readonly TimeSpan _flushInterval;
        private readonly TimeSpan _minBackoffPeriod;
        private readonly TimeSpan _maxBackoffPeriod;

        public TimerStatus(TimeSpan interval, TimeSpan minBackoffPeriod, TimeSpan maxBackoffPeriod)
        {
            if (interval < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(interval)} must be a positive TimeSpan.", nameof(interval));

            if (minBackoffPeriod < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(minBackoffPeriod)} must be a positive TimeSpan.", nameof(minBackoffPeriod));

            if (maxBackoffPeriod < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(maxBackoffPeriod)} must be a positive TimeSpan.", nameof(maxBackoffPeriod));

            _flushInterval = interval;
            _minBackoffPeriod = minBackoffPeriod;
            _maxBackoffPeriod = maxBackoffPeriod;
        }

        public int RecentFailures { get; private set; }

        public void MarkSuccess()
        {
            RecentFailures = 0;
        }

        public void MarkFailure()
        {
            ++RecentFailures;
        }

        public TimeSpan NextInterval
        {
            get
            {
                // Available, and first failure, just try the batch interval
                if (RecentFailures <= 1)
                    return _flushInterval;

                // Second failure, start ramping up the interval - first 2x, then 4x, ...
                var backoffFactor = Math.Pow(2, (RecentFailures - 1));

                // If the period is ridiculously short, give it a boost so we get some
                // visible backoff.
                var backoffPeriod = Math.Max(_flushInterval.Ticks, _minBackoffPeriod.Ticks);

                // The "ideal" interval
                var idealBackoff = (long) (backoffPeriod * backoffFactor);

                // ... capped to the maximum interval
                var cappedBackoff = Math.Min(_maxBackoffPeriod.Ticks, idealBackoff);

                // ... unless that's shorter than the period, in which case we'll just apply the period
                var actual = Math.Max(_flushInterval.Ticks, cappedBackoff);

                return TimeSpan.FromTicks(actual);
            }
        }
    }
}