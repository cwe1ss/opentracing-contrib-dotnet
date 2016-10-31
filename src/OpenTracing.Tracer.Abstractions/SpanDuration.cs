using System;

namespace OpenTracing.Tracer.Abstractions
{
    /// <summary>
    /// A helper type for managing a duration that uses either system timestamps
    /// or user-supplied timestamps.
    /// </summary>
    public struct SpanDuration
    {
        private readonly IClock _clock;

        public DateTime StartTimestamp { get; }

        public SpanDuration(IClock clock, DateTime? userSuppliedStartTimestamp)
        {
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));

            if (userSuppliedStartTimestamp.HasValue)
            {
                EnsureUtc(userSuppliedStartTimestamp.Value);

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

        public DateTime GetTimestamp(DateTime? userSuppliedTimestamp)
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

                EnsureUtc(userSuppliedTimestamp.Value);
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

        private void EnsureGreaterThanStart(DateTime timestamp)
        {
            if (timestamp < StartTimestamp)
            {
                throw new InvalidOperationException("The user-supplied timestamp must be higher than the start time.");
            }
        }

        private static void EnsureUtc(DateTime timestamp)
        {
            if (timestamp.Kind != DateTimeKind.Utc)
            {
                throw new InvalidOperationException("The kind for user-supplied timestamps must be 'Utc'.");
            }
        }
    }
}