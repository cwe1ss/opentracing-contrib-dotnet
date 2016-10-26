using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public abstract class SpanBuilderBase : ISpanBuilder
    {
        private List<SpanReference> _references;

        private Dictionary<string, bool> _boolTags;
        private Dictionary<string, double> _doubleTags;
        private Dictionary<string, int> _intTags;
        private Dictionary<string, string> _stringTags;

        protected string OperationName { get; }

        protected DateTime? StartTimestamp { get; private set; }

        protected IEnumerable<SpanReference> SpanReferences
        {
            get { return _references ?? Enumerable.Empty<SpanReference>(); }
        }

        protected IEnumerable<KeyValuePair<string, bool>> BoolTags
        {
            get { return _boolTags ?? Enumerable.Empty<KeyValuePair<string, bool>>(); }
        }

        protected IEnumerable<KeyValuePair<string, double>> DoubleTags
        {
            get { return _doubleTags ?? Enumerable.Empty<KeyValuePair<string, double>>(); }
        }

        protected IEnumerable<KeyValuePair<string, int>> IntTags
        {
            get { return _intTags ?? Enumerable.Empty<KeyValuePair<string, int>>(); }
        }

        protected IEnumerable<KeyValuePair<string, string>> StringTags
        {
            get { return _stringTags ?? Enumerable.Empty<KeyValuePair<string, string>>(); }
        }

        protected IEnumerable<KeyValuePair<string, object>> AllTags
        {
            get
            {
                // TODO @cweiss Remove this?

                int capacity = (_boolTags?.Count ?? 0) + (_doubleTags?.Count ?? 0) + (_intTags?.Count ?? 0) + (_stringTags?.Count ?? 0);

                if (capacity == 0)
                {
                    return Enumerable.Empty<KeyValuePair<string, object>>();
                }

                Dictionary<string, object> allTags = new Dictionary<string, object>(capacity);

                if (_boolTags != null)
                {
                    foreach (var kvp in _boolTags)
                    {
                        allTags.Add(kvp.Key, kvp.Value);
                    }
                }

                if (_doubleTags != null)
                {
                    foreach (var kvp in _doubleTags)
                    {
                        allTags.Add(kvp.Key, kvp.Value);
                    }
                }

                if (_intTags != null)
                {
                    foreach (var kvp in _intTags)
                    {
                        allTags.Add(kvp.Key, kvp.Value);
                    }
                }

                if (_stringTags != null)
                {
                    foreach (var kvp in _stringTags)
                    {
                        allTags.Add(kvp.Key, kvp.Value);
                    }
                }

                return allTags;
            }
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

            _boolTags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_doubleTags == null)
                _doubleTags = new Dictionary<string, double>();

            _doubleTags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_intTags == null)
                _intTags = new Dictionary<string, int>();

            _intTags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_stringTags == null)
                _stringTags = new Dictionary<string, string>();

            _stringTags.Add(key, value);
            return this;
        }

        public ISpan Start()
        {
            return CreateSpan();
        }

        protected abstract SpanBase CreateSpan();
    }
}