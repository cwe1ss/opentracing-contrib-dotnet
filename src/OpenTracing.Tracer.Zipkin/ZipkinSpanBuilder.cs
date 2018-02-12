using System;
using System.Linq;
using OpenTracing.Tag;
using OpenTracing.Tracer;

namespace OpenTracing.Tracer.Zipkin
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

            // Only one reference is supported right now - the rest is ignored.
            var parent = (ZipkinSpanContext)SpanReferences.FirstOrDefault()?.ReferencedContext;

            if (parent != null)
            {
                return parent.CreateChild(spanId);
            }
            else
            {
                // This is a root span!

                bool sampled = false;

                if (IntTags.Any(x => x.Key == Tags.SamplingPriority && x.Value == 1))
                {
                    sampled = true;
                }
                else if (_tracer.Sampler.IsSampled(spanId))
                {
                    sampled = true;

                    foreach (var tag in _tracer.Sampler.GetTags())
                        WithTag(tag.Key, tag.Value);
                }

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
