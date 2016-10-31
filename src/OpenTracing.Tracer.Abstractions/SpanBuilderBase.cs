using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer.Abstractions
{
    public abstract class SpanBuilderBase : ISpanBuilder
    {
        // Variables are only allocated if there are references/tags.
        // This makes sure we don't do unnecessary allocations.

        private List<SpanReference> _references;

        // Separate dictionaries to prevent boxing.
        private Dictionary<string, bool> _boolTags;
        private Dictionary<string, double> _doubleTags;
        private Dictionary<string, int> _intTags;
        private Dictionary<string, string> _stringTags;

        protected string OperationName { get; }

        protected DateTime? StartTimestamp { get; private set; }

        /// <summary>
        /// A (potentially empty) list of span references. This will never return <c>null</c>.
        /// </summary>
        protected IEnumerable<SpanReference> SpanReferences
        {
            get { return _references ?? Enumerable.Empty<SpanReference>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>bool</c> tags. This will never return <c>null</c>.
        /// </summary>
        protected IEnumerable<KeyValuePair<string, bool>> BoolTags
        {
            get { return _boolTags ?? Enumerable.Empty<KeyValuePair<string, bool>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>double</c> tags. This will never return <c>null</c>.
        /// </summary>
        protected IEnumerable<KeyValuePair<string, double>> DoubleTags
        {
            get { return _doubleTags ?? Enumerable.Empty<KeyValuePair<string, double>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>int</c> tags. This will never return <c>null</c>.
        /// </summary>
        protected IEnumerable<KeyValuePair<string, int>> IntTags
        {
            get { return _intTags ?? Enumerable.Empty<KeyValuePair<string, int>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>string</c> tags. This will never return <c>null</c>.
        /// </summary>
        protected IEnumerable<KeyValuePair<string, string>> StringTags
        {
            get { return _stringTags ?? Enumerable.Empty<KeyValuePair<string, string>>(); }
        }

        protected SpanBuilderBase(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            OperationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
                throw new ArgumentNullException(nameof(referenceType));

            if (referencedContext == null)
                return this;

            if (_references == null)
                _references = new List<SpanReference>();

            _references.Add(new SpanReference(referenceType, referencedContext));
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
            StartTimestamp = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_boolTags == null)
                _boolTags = new Dictionary<string, bool>();

            _boolTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_doubleTags == null)
                _doubleTags = new Dictionary<string, double>();

            _doubleTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_intTags == null)
                _intTags = new Dictionary<string, int>();

            _intTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_stringTags == null)
                _stringTags = new Dictionary<string, string>();

            _stringTags[key] = value;
            return this;
        }

        public abstract ISpan Start();

        /// <summary>
        /// Helper method for passing all tags to the given span
        /// in case tags are not needed for the span constructor.
        /// </summary>
        protected void SetSpanTags(ISpan span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            foreach (var tag in BoolTags)
                span.SetTag(tag.Key, tag.Value);

            foreach (var tag in DoubleTags)
                span.SetTag(tag.Key, tag.Value);

            foreach (var tag in IntTags)
                span.SetTag(tag.Key, tag.Value);

            foreach (var tag in StringTags)
                span.SetTag(tag.Key, tag.Value);
        }
    }
}