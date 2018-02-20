using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenTracing.Tag;
using OpenTracing.Tracer;
using zipkin4net;
using zipkin4net.Annotation;

namespace OpenTracing.Tracer.Zipkin
{
    internal class OtSpanBuilder : ISpanBuilder
    {
        private readonly OtTracer _tracer;
        private readonly string _operationName;

        private DateTimeOffset _startTimestamp;
        private bool _ignoreActiveSpan;
        private Dictionary<string, string> _tags;
        private OtSpanContext _parent;

        public OtSpanBuilder(OtTracer tracer, string operationName)
        {
            _tracer = tracer;
            _operationName = operationName;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            return AddReference(References.ChildOf, parent);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            return AddReference(References.ChildOf, parent?.Context);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            // Only one reference is supported for now.
            if (_parent != null)
                return this;

            if (referenceType == References.ChildOf || referenceType == References.FollowsFrom)
            {
                _parent = (OtSpanContext)referencedContext;
            }

            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            _startTimestamp = timestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            if (_tags == null)
                _tags = new Dictionary<string, string>();

            _tags[key] = value;

            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            return WithTag(key, value ? "1" : "0");
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            return WithTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            return WithTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpan Start()
        {
            OtSpanKind spanKind = GetSpanKind();
            Trace trace = GetCurrentTraceAccordingToSpanKind(spanKind);

            if (!_ignoreActiveSpan)
            {
                // TODO _parent && ActiveSpan !!!
            }

            // Forcing sampling on child spans would lead to inconsistent data.
            if (_parent == null)
            {
                var forceSampling = IsSamplingForced();
                if (forceSampling)
                {
                    trace.ForceSampled();
                }
            }

            var annotation = GetOpeningAnnotation(spanKind, _operationName);

            if (_startTimestamp != default(DateTimeOffset))
            {
                trace.Record(annotation, _startTimestamp.UtcDateTime);
            }
            else
            {
                trace.Record(annotation);
            }

            if (_operationName != null && spanKind != OtSpanKind.Local)
            {
                trace.Record(Annotations.ServiceName(_operationName));
            }

            if (_tags != null)
            {
                foreach (var entry in _tags)
                {
                    if (entry.Key.Equals(Tags.SpanKind))
                        continue;

                    trace.Record(Annotations.Tag(entry.Key, entry.Value));
                }
            }

            return new OtSpan(trace, spanKind);
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            ISpan span = Start();

            return _tracer.ScopeManager.Activate(span, finishSpanOnDispose);
        }

        private static Trace GetCurrentTraceAccordingToSpanKind(OtSpanKind spanKind)
        {
            var trace = Trace.Current;
            switch (spanKind)
            {
                case OtSpanKind.Server:
                    {
                        if (trace == null)
                        {
                            trace = Trace.Create();
                            Trace.Current = trace;
                        }
                        break;
                    }
                case OtSpanKind.Client:
                case OtSpanKind.Local:
                    {
                        trace = (trace == null ? Trace.Create() : trace.Child());
                        Trace.Current = trace;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return trace;
        }

        private bool IsSamplingForced()
        {
            if (_tags != null && _tags.TryGetValue(Tags.SamplingPriority.Key, out string sampling) && sampling != default(string))
            {
                try
                {
                    var samplingPriority = int.Parse(sampling);
                    return samplingPriority > 0;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return false;
        }

        private OtSpanKind GetSpanKind()
        {
            if (_tags != null && _tags.TryGetValue(Tags.SpanKind.Key, out string spanKind))
            {
                return Tags.SpanKindClient.Equals(spanKind) ? OtSpanKind.Client : OtSpanKind.Server;
            }
            else
            {
                return OtSpanKind.Local;
            }
        }

        private static IAnnotation GetOpeningAnnotation(OtSpanKind spanKind, string operationName)
        {
            switch (spanKind)
            {
                case OtSpanKind.Client:
                    return Annotations.ClientSend();
                case OtSpanKind.Server:
                    return Annotations.ServerRecv();
                case OtSpanKind.Local:
                    {
                        if (operationName == null)
                        {
                            throw new NullReferenceException("Trying to start a local span without any operation name is forbidden");
                        }

                        return Annotations.LocalOperationStart(operationName);
                    }
            }
            throw new NotSupportedException("SpanKind: " + spanKind + " unknown.");
        }
    }
}
