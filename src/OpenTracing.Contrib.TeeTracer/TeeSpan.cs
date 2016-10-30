using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.TeeTracer
{
    public class TeeSpan : ISpan
    {
        private readonly ISpan _mainSpan;
        private readonly ISpan _secondarySpan;

        public ISpanContext Context => _mainSpan.Context;

        public TeeSpan(ISpan mainSpan, ISpan secondarySpan)
        {
            if (mainSpan == null)
                throw new ArgumentNullException(nameof(mainSpan));

            _mainSpan = mainSpan;
            _secondarySpan = secondarySpan;
        }

        public ISpan SetOperationName(string operationName)
        {
            _mainSpan.SetOperationName(operationName);
            _secondarySpan?.SetOperationName(operationName);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            string value = _mainSpan.GetBaggageItem(key);
            _secondarySpan?.GetBaggageItem(key);
            return value;
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            _mainSpan.SetBaggageItem(key, value);
            _secondarySpan?.SetBaggageItem(key, value);
            return this;
        }

        public ISpan Log(string eventName)
        {
            _mainSpan.Log(eventName);
            _secondarySpan?.Log(eventName);
            return this;
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            _mainSpan.Log(timestamp, eventName);
            _secondarySpan?.Log(timestamp, eventName);
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            _mainSpan.Log(fields);
            _secondarySpan?.Log(fields);
            return this;
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            _mainSpan.Log(timestamp, fields);
            _secondarySpan?.Log(timestamp, fields);
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            _mainSpan.SetTag(key, value);
            _secondarySpan?.SetTag(key, value);
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            _mainSpan.SetTag(key, value);
            _secondarySpan?.SetTag(key, value);
            return this;
        }

        public ISpan SetTag(string key, int value)
        {
            _mainSpan.SetTag(key, value);
            _secondarySpan?.SetTag(key, value);
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            _mainSpan.SetTag(key, value);
            _secondarySpan?.SetTag(key, value);
            return this;
        }

        public void Finish()
        {
            _mainSpan.Finish();
            _secondarySpan?.Finish();
        }

        public void Finish(DateTime finishTimestamp)
        {
            _mainSpan.Finish(finishTimestamp);
            _secondarySpan?.Finish(finishTimestamp);
        }

        public void Dispose()
        {
            _mainSpan.Dispose();
            _secondarySpan?.Dispose();
        }
    }
}