using System;
using System.Linq;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinSpanBuilder : SpanBuilderBase
    {
        private static readonly Random _random = new Random();

        private readonly ZipkinTracer _tracer;

        public ZipkinSpanBuilder(ZipkinTracer tracer, string operationName)
            : base(operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            _tracer = tracer;
        }

        public override ISpan Start()
        {
            ZipkinSpanContext context = GetOrCreateContext();

            var span = new ZipkinSpan(_tracer, context, OperationName, StartTimestamp);

            SetSpanTags(span);

            return span;
        }

        private ZipkinSpanContext GetOrCreateContext()
        {
            ulong spanId = GetRandomId();

            // TODO @cweiss referenceType, multiple references ?
            int referenceCount = SpanReferences.Count();

            if (referenceCount > 1)
            {
                throw new NotSupportedException("Only one parent is supported right now");
            }

            if (referenceCount == 1)
            {
                // This is a child-span!

                var parent = (ZipkinSpanContext)SpanReferences.First().ReferencedContext;
                return parent.CreateChild(spanId);
            }
            else
            {
                // This is a root span!

                // TODO @cweiss Sampling important here?!?
                bool sampled = IntTags.Any(x => x.Key == Tags.SamplingPriority && x.Value == 1);

                // TraceId and SpanId may be equal: http://zipkin.io/pages/instrumenting.html
                return new ZipkinSpanContext(spanId, spanId, parentId: null, sampled: sampled, baggage: null);
            }
        }

        private static ulong GetRandomId()
        {
            // http://stackoverflow.com/questions/677373/generate-random-values-in-c-sharp
            byte[] bytes = new byte[8];
            _random.NextBytes(bytes);
            ulong number = BitConverter.ToUInt64(bytes, 0);
            return number;
        }
    }
}