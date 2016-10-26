using System;
using System.Linq;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinSpanBuilder : SpanBuilderBase
    {
        private readonly ZipkinTracer _tracer;

        public ZipkinSpanBuilder(ZipkinTracer tracer, string operationName)
            : base(operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            _tracer = tracer;
        }

        protected override SpanBase CreateSpan()
        {
            ZipkinSpanContext context = GetOrCreateContext();

            return new ZipkinSpan(
                _tracer,
                context,
                OperationName,
                StartTimestamp,
                AllTags // TODO @cweiss use separate type tags
            );
        }

        private ZipkinSpanContext GetOrCreateContext()
        {
            ulong spanId = (ulong)BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

            // TODO @cweiss referenceType, multiple references ?
            int referenceCount = SpanReferences.Count();

            if (referenceCount > 1)
            {
                throw new NotSupportedException("Only one parent is supported right now");
            }

            if (referenceCount == 1)
            {
                // This is a child-span!

                var parent = (ZipkinSpanContext) SpanReferences.First().ReferencedContext;
                return parent.CreateChild(spanId);
            }

            // This is a root span!

            // TraceId and SpanId may be equal: http://zipkin.io/pages/instrumenting.html
            return new ZipkinSpanContext(spanId, spanId, parentId: null, baggage: null);
        }
    }
}