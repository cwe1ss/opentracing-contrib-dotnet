using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class SpanBuilder : ISpanBuilder
    {
        private readonly ZipkinTracer _tracer;
        private readonly string _operationName;

        private DateTime? _startTimestamp;
        private List<KeyValuePair<string, ISpanContext>> _references;

        // TODO @cweiss prevent boxing with multiple lists? list or dictionary?
        private List<KeyValuePair<string, object>> _tags;

        public SpanBuilder(ZipkinTracer tracer, string operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            _operationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referenceType == null)
                throw new ArgumentNullException(nameof(referenceType));

            if (referencedContext == null)
                return this;

            if (_references == null)
                _references = new List<KeyValuePair<string, ISpanContext>>();

            _references.Add(new KeyValuePair<string, ISpanContext>(referenceType, referencedContext));
            return this;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            AddReference(References.ChildOf, parent);
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            AddReference(References.ChildOf, parent?.Context);
            return this;
        }

        public ISpanBuilder FollowsFrom(ISpanContext parent)
        {
            AddReference(References.FollowsFrom, parent);
            return this;
        }

        public ISpanBuilder FollowsFrom(ISpan parent)
        {
            AddReference(References.FollowsFrom, parent?.Context);
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTime startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            return AddTag(key, value);
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            return AddTag(key, value);
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            return AddTag(key, value);
        }

        public ISpan Start()
        {
            SpanContext context = GetOrCreateContext();

            return new Span(
                _tracer,
                context,
                _operationName,
                _startTimestamp,
                _tags
            );
        }

        private SpanContext GetOrCreateContext()
        {
            ulong spanId = (ulong)BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

            // TODO @cweiss referenceType, multiple references ?

            if (_references?.Count > 1)
            {
                throw new NotSupportedException("Only one parent is supported right now");
            }

            if (_references?.Count == 1)
            {
                // This is a child-span!

                var parent = (SpanContext) _references[0].Value;
                return parent.CreateChild(spanId);
            }

            // This is a root span!

            // TraceId and SpanId may be equal: http://zipkin.io/pages/instrumenting.html
            return new SpanContext(spanId, spanId, parentId: null, baggage: null);
        }

        private ISpanBuilder AddTag(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_tags == null)
                _tags = new List<KeyValuePair<string, object>>();

            _tags.Add(new KeyValuePair<string, object>(key, value));
            return this;
        }
    }
}