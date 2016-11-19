using System;

namespace OpenTracing.Tracer.BatchReporter
{
    public class BatchReporterOptions
    {
        public int MaxQueueSize { get; set; }
        public int MaxBatchSize { get; set; }

        public TimeSpan FlushInterval { get; set; }

        public TimeSpan MinBackoffPeriod { get; set; }
        public TimeSpan MaxBackoffPeriod { get; set; }

        public int FailuresBeforeDroppingBatch { get; set; }
        public int FailuresBeforeDroppingQueue { get; set; }

        public BatchReporterOptions()
        {
            MaxQueueSize = 1000;
            MaxBatchSize = 50;
            FlushInterval = TimeSpan.FromSeconds(2);

            MinBackoffPeriod = TimeSpan.FromSeconds(5);
            MaxBackoffPeriod = TimeSpan.FromMinutes(1);

            FailuresBeforeDroppingBatch = 4;
            FailuresBeforeDroppingQueue = 8;
        }

        public virtual void Validate()
        {
            if (MaxQueueSize < 1)
                throw new ArgumentException($"{nameof(MaxQueueSize)} must be greater than zero.", nameof(MaxQueueSize));

            if (MaxBatchSize < 1)
                throw new ArgumentException($"{nameof(MaxBatchSize)} must be greater than zero.", nameof(MaxBatchSize));

            if (FlushInterval < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(FlushInterval)} must be a positive TimeSpan.", nameof(FlushInterval));

            if (MinBackoffPeriod < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(MinBackoffPeriod)} must be a positive TimeSpan.", nameof(MinBackoffPeriod));

            if (MaxBackoffPeriod < TimeSpan.Zero)
                throw new ArgumentException($"{nameof(MaxBackoffPeriod)} must be a positive TimeSpan.", nameof(MaxBackoffPeriod));

            if (FailuresBeforeDroppingBatch < 0)
                throw new ArgumentException($"{nameof(FailuresBeforeDroppingBatch)} may not be negative.", nameof(FailuresBeforeDroppingBatch));

            if (FailuresBeforeDroppingQueue < 0)
                throw new ArgumentException($"{nameof(FailuresBeforeDroppingQueue)} may not be negative.", nameof(FailuresBeforeDroppingQueue));
        }
    }
}