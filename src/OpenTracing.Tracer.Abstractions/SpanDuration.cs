using System;

namespace OpenTracing.Tracer
{
    /// <summary>
    /// A helper type for managing a duration that uses either system timestamps
    /// or user-supplied timestamps.
    /// </summary>
    public struct SpanDuration
    {
        private readonly IClock _clock;

        public DateTimeOffset StartTimestamp { get; }

        public SpanDuration(IClock clock, DateTimeOffset? userSuppliedStartTimestamp)
        {
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));

            if (userSuppliedStartTimestamp.HasValue)
            {
                // User-supplied timestamps! We don't need the clock.
                _clock = null;
                StartTimestamp = userSuppliedStartTimestamp.Value;
            }
            else
            {
                // System time should be used!
                _clock = clock;
                StartTimestamp = clock.UtcNow;
            }
        }

        public DateTimeOffset GetTimestamp(DateTimeOffset? userSuppliedTimestamp)
        {
            if (_clock == null)
            {
                if (!userSuppliedTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "There was an attempt to get the current system timestamp from a span " +
                        "that has been started with a user-supplied timestamp. " +
                        "All events for such a span must provide a user-supplied timestamp.");
                }

                EnsureGreaterThanStart(userSuppliedTimestamp.Value);

                return userSuppliedTimestamp.Value;
            }
            else
            {
                if (userSuppliedTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "There was an attempt to use a user-supplied timestamp for a span " +
                        "that has been started with a system timestamp. " +
                        "All events for such a span must use a system timestamp.");
                }

                return _clock.UtcNow;
            }
        }

        private void EnsureGreaterThanStart(DateTimeOffset timestamp)
        {
            if (timestamp < StartTimestamp)
            {
                throw new InvalidOperationException("The user-supplied timestamp must be higher than the start time.");
            }
        }
    }
}
