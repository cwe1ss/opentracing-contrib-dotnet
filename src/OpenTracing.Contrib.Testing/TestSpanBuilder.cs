using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.Testing
{
    public class TestSpanBuilder : ISpanBuilder
    {
        public TestTracer Tracer { get; }
        public string OperationName { get; }
        public List<KeyValuePair<string, ISpanContext>> References { get; private set; }
        public DateTime? StartTimestamp { get; private set; }
        public Dictionary<string, object> Tags { get; private set; }

        public TestSpanBuilder(TestTracer tracer, string operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (operationName == null)
                throw new ArgumentNullException(nameof(operationName));

            Tracer = tracer;
            OperationName = operationName;
            References = new List<KeyValuePair<string, ISpanContext>>();
            Tags = new Dictionary<string, object>();
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            References.Add(new KeyValuePair<string, ISpanContext>(referenceType, referencedContext));
            return this;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            return AddReference(OpenTracing.References.ChildOf, parent);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            return AddReference(OpenTracing.References.ChildOf, parent?.Context);
        }

        public ISpanBuilder FollowsFrom(ISpanContext parent)
        {
            return AddReference(OpenTracing.References.FollowsFrom, parent);
        }

        public ISpanBuilder FollowsFrom(ISpan parent)
        {
            return AddReference(OpenTracing.References.FollowsFrom, parent?.Context);
        }

        public ISpanBuilder WithStartTimestamp(DateTime startTimestamp)
        {
            StartTimestamp = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan Start()
        {
            TestSpanContext spanContext;

            if (References.Count > 0)
            {
                spanContext = (TestSpanContext)References[0].Value;
            }
            else
            {
                spanContext = new TestSpanContext();
            }

            return new TestSpan(
                Tracer,
                spanContext,
                StartTimestamp ?? DateTime.UtcNow,
                OperationName,
                References,
                Tags);
        }
    }
}