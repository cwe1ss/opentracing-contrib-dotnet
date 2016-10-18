using System;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class HighResDuration
    {
        private readonly HighResClock _clock = null;

        public DateTime StartTimestamp { get; }

        public DateTime? FinishTimestamp { get; private set; }

        public HighResDuration(HighResClock clock, DateTime? startTimestamp)
        {
            if (startTimestamp.HasValue)
            {
                if (startTimestamp.Value.Kind != DateTimeKind.Utc)
                {
                    throw new InvalidOperationException("The kind for user-supplied timestamps must be 'Utc'.");
                }

                // User-supplied timestamps! We don't store the clock.
                StartTimestamp = startTimestamp.Value;
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
                    "There was an attempt to get the current UTC timestamp from a span " +
                    "that has been started with a user-supplied timestamp. " + 
                    "All events for such a span must also provide a custom timestamp.");
            }

            return _clock.UtcNow;
        }

        public void Finish(DateTime? finishTimestamp)
        {
            if (_clock == null)
            {
                if (!finishTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "The span may not be finished with a system timestamp, " + 
                        "if it was started with a user-supplied timestamp.");
                }

                if (finishTimestamp.Value.Kind != DateTimeKind.Utc)
                {
                    throw new InvalidOperationException("The kind for user-supplied timestamps must be 'Utc'.");
                }

                FinishTimestamp = finishTimestamp;
            }
            else
            {
                if (finishTimestamp.HasValue)
                {
                    throw new InvalidOperationException(
                        "The span may not be finished with a user-supplied timestamp, " +
                        "if it was started with a system timestamp.");
                }

                FinishTimestamp = _clock.UtcNow;
            }
        }
    }
}