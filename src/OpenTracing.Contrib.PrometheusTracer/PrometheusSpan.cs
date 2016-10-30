using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.PrometheusTracer
{
    public class PrometheusSpan : ISpan
    {
        public ISpanContext Context => PrometheusSpanContext.Instance;

        public ISpan SetOperationName(string operationName)
        {
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return null;
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            return this;
        }

        public ISpan Log(string eventName)
        {
            return this;
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return this;
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            return this;
        }

        public ISpan SetTag(string key, int value)
        {
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            return this;
        }

        public void Finish()
        {
            throw new NotImplementedException();
        }

        public void Finish(DateTime finishTimestamp)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}