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
    internal class OtSpan : ISpan
    {
        private readonly Trace _trace;
        private readonly OtSpanKind _spanKind;
        private bool _isFinished;

        public ISpanContext Context { get; }

        public OtSpan(Trace trace, OtSpanKind spanKind)
        {
            _trace = trace;
            _spanKind = spanKind;
            Context = new OtSpanContext(trace);
        }

        public ISpan SetOperationName(string operationName)
        {
            _trace.Record(Annotations.ServiceName(operationName));
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            _trace.Record(Annotations.Tag(key, value));
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return SetTag(key, value ? "1" : "0");
        }

        public ISpan SetTag(string key, int value)
        {
            return SetTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpan SetTag(string key, double value)
        {
            return SetTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpan Log(IDictionary<string, object> fields)
        {
            _trace.Record(Annotations.Event(JoinKeyValuePairs(fields)));
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            _trace.Record(Annotations.Event(JoinKeyValuePairs(fields)), timestamp.UtcDateTime);
            return this;
        }

        public ISpan Log(string @event)
        {
            _trace.Record(Annotations.Event(@event));
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            _trace.Record(Annotations.Event(@event), timestamp.UtcDateTime);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            throw new NotImplementedException();
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Finish()
        {
            if (!_isFinished)
            {
                _trace.Record(GetClosingAnnotation(_spanKind));
                _isFinished = true;
            }
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            if (!_isFinished)
            {
                _trace.Record(GetClosingAnnotation(_spanKind), finishTimestamp.UtcDateTime);
                _isFinished = true;
            }
        }

        private static string JoinKeyValuePairs(IDictionary<string, object> fields)
        {
            return string.Join(" ", fields.Select(entry => entry.Key + ":" + entry.Value));
        }

        private static IAnnotation GetClosingAnnotation(OtSpanKind spanKind)
        {
            switch (spanKind)
            {
                case OtSpanKind.Client:
                    return Annotations.ClientRecv();
                case OtSpanKind.Server:
                    return Annotations.ServerSend();
                case OtSpanKind.Local:
                    return Annotations.LocalOperationStop();
                default:
                    throw new NotSupportedException("SpanKind: " + spanKind + " unknown.");
            }
        }
    }
}
