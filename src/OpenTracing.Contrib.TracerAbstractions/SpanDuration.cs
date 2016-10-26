using System;

namespace OpenTracing.Contrib.TracerAbstractions
{
    /// <summary>
    /// A helper type for managing a duration that uses either system timestamps
    /// or user-supplied timestamps.
    /// </summary>
    public struct SpanDuration
    {
        private readonly HighResClock _clock;

        public DateTime StartTimestamp { get; }

        public SpanDuration(HighResClock clock, DateTime? userSuppliedStartTimestamp)
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

        public DateTime GetUtcNow()
        {
            if (_clock == null)
            {
                throw new InvalidOperationException(
                    "There was an attempt to get the current system timestamp from a span " +
                    "that has been started with a user-supplied timestamp. " +
                    "All events for such a span must also provide a user-supplied timestamp.");
            }

            return _clock.UtcNow;
        }

        public void ValidateTimestamp(DateTime timestamp)
        {
            EnsureUtc(timestamp);
            EnsureGreaterThanStart(timestamp);
        }

        public DateTime Finish(DateTime? userSuppliedFinishTimestamp)
        {
            if (_clock == null)
            {
                if (!userSuppliedFinishTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "The span may not be finished with a system timestamp " +
                        "if it was started with a user-supplied timestamp.");
                }

                EnsureUtc(userSuppliedFinishTimestamp.Value);
                EnsureGreaterThanStart(userSuppliedFinishTimestamp.Value);

                return userSuppliedFinishTimestamp.Value;
            }
            else
            {
                if (userSuppliedFinishTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "The span may not be finished with a user-supplied timestamp " +
                        "if it was started with a system timestamp.");
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