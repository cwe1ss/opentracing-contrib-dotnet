using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer
{
    public abstract class SpanBuilderBase : ISpanBuilder
    {
        private readonly TracerBase _tracer;

        private bool _ignoreActiveSpan;

        protected string OperationName { get; }

        protected DateTimeOffset? StartTimestamp { get; private set; }

        /// <summary>
        /// A (potentially <c>null</c>) list of span references. This will only get allocated when there's at least one reference.
        /// </summary>
        protected KeyValueListNode<ISpanContext> SpanReferences { get; private set; }

        /// <summary>
        /// A (potentially <c>null</c>) list with tags. This will only get allocated when there's at least one tag.
        /// </summary>
        protected KeyValueListNode<object> SpanTags { get; private set; }

        protected SpanBuilderBase(TracerBase tracer, string operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            OperationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referencedContext == null)
                return this;

            SpanReferences = new KeyValueListNode<ISpanContext>()
            {
                KeyValue = new KeyValuePair<string, ISpanContext>(referenceType, referencedContext),
                Next = SpanReferences
            };
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

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            SpanTags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = SpanTags };
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            SpanTags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = SpanTags };
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            SpanTags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = SpanTags };
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            SpanTags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = SpanTags };
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            StartTimestamp = startTimestamp;
            return this;
        }

        public IScope StartActive(bool finishOnDispose)
        {

        }

        public ISpan Start()
        {
            ISpan parent = null;

            if (!_ignoreActiveSpan)
            {
                parent = _tracer.ActiveSpan;
                AsChildOf(parent);
            }

            var span = CreateSpan();

            span.Parent = parent;

            _tracer.ActiveSpan = span;

            return span;
        }

        protected abstract SpanBase CreateSpan();
    }
}
