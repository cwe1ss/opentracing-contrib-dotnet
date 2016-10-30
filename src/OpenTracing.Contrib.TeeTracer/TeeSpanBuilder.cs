using System;

namespace OpenTracing.Contrib.TeeTracer
{
    public class TeeSpanBuilder : ISpanBuilder
    {
        private readonly ISpanBuilder _mainBuilder;
        private readonly ISpanBuilder _secondaryBuilder;

        public TeeSpanBuilder(ISpanBuilder mainSpanBuilder, ISpanBuilder secondarySpanBuilder)
        {
            if (mainSpanBuilder == null)
                throw new ArgumentNullException(nameof(mainSpanBuilder));

            _mainBuilder = mainSpanBuilder;
            _secondaryBuilder = secondarySpanBuilder;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            _mainBuilder.AddReference(referenceType, referencedContext);
            _secondaryBuilder?.AddReference(referenceType, referencedContext);
            return this;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            _mainBuilder.AsChildOf(parent);
            _secondaryBuilder?.AsChildOf(parent);
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            _mainBuilder.AsChildOf(parent);
            _secondaryBuilder?.AsChildOf(parent);
            return this;
        }

        public ISpanBuilder FollowsFrom(ISpanContext parent)
        {
            _mainBuilder.FollowsFrom(parent);
            _secondaryBuilder?.FollowsFrom(parent);
            return this;
        }

        public ISpanBuilder FollowsFrom(ISpan parent)
        {
            _mainBuilder.FollowsFrom(parent);
            _secondaryBuilder?.FollowsFrom(parent);
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTime startTimestamp)
        {
            _mainBuilder.WithStartTimestamp(startTimestamp);
            _secondaryBuilder?.WithStartTimestamp(startTimestamp);
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            _mainBuilder.WithTag(key, value);
            _secondaryBuilder?.WithTag(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            _mainBuilder.WithTag(key, value);
            _secondaryBuilder?.WithTag(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            _mainBuilder.WithTag(key, value);
            _secondaryBuilder?.WithTag(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            _mainBuilder.WithTag(key, value);
            _secondaryBuilder?.WithTag(key, value);
            return this;
        }

        public ISpan Start()
        {
            var mainSpan = _mainBuilder.Start();
            var secondarySpan = _secondaryBuilder?.Start();

            return new TeeSpan(mainSpan, secondarySpan);
        }
    }
}