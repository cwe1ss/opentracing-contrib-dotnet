using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTracing.Tracer.Abstractions;
using OpenTracing.Propagation;

namespace OpenTracing.Tracer.Zipkin.Propagation
{
    public class TextMapPropagator : IPropagator
    {
        private const string IdFormat = "x4";

        // http://zipkin.io/pages/instrumenting.html
        private const string TraceIdHeader = "X-B3-TraceId";
        private const string SpanIdHeader = "X-B3-SpanId";
        private const string ParentIdHeader = "X-B3-ParentSpanId";
        private const string SampledHeader = "X-B3-Sampled";

        private const string BaggageHeaderPrefix = "X-Baggage-"; // TODO @cweiss is this defined somewhere???

        private const string SampledTrue = "1";
        private const string SampledFalse = "0";

        // TODO @cweiss Is URL Encoding required with .NET header types?

        public void Inject(ISpanContext untypedContext, object carrier)
        {
            var context = (ZipkinSpanContext)untypedContext;

            var textMap = carrier as ITextMap;
            if (textMap == null)
                throw new InvalidOperationException($"Carrier must be a '{nameof(ITextMap)}'. Actual type: '{carrier?.GetType()}'.");

            textMap.Set(TraceIdHeader, context.TraceId.ToString(IdFormat));
            textMap.Set(SpanIdHeader, context.SpanId.ToString(IdFormat));
            textMap.Set(SampledHeader, context.Sampled ? SampledTrue : SampledFalse);

            if (context.ParentId.HasValue)
                textMap.Set(ParentIdHeader, context.ParentId.Value.ToString(IdFormat));

            // TODO @cweiss Baggage Encoding?!
            foreach (var baggage in context.GetBaggageItems())
            {
                textMap.Set(BaggageHeaderPrefix + baggage.Key, baggage.Value);
            }
        }

        public ISpanContext Extract(object carrier)
        {
            var textMap = carrier as ITextMap;
            if (textMap == null)
                throw new InvalidOperationException($"Carrier must be a '{nameof(ITextMap)}'. Actual type: '{carrier?.GetType()}'.");

            ulong traceId = 0, spanId = 0, parentId = 0;
            bool sampled = false;
            Dictionary<string, string> baggage = null;

            foreach (var entry in textMap.GetEntries())
            {
                if (string.Equals(entry.Key, TraceIdHeader, StringComparison.OrdinalIgnoreCase))
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out traceId);
                }
                else if (string.Equals(entry.Key, SpanIdHeader, StringComparison.OrdinalIgnoreCase))
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out spanId);
                }
                else if (string.Equals(entry.Key, ParentIdHeader, StringComparison.OrdinalIgnoreCase))
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parentId);
                }
                else if (string.Equals(entry.Key, SampledHeader, StringComparison.OrdinalIgnoreCase))
                {
                    sampled = entry.Value == SampledTrue;
                }
                else if (entry.Key.StartsWith(BaggageHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (baggage == null)
                        baggage = new Dictionary<string, string>();

                    baggage.Add(entry.Key.Substring(BaggageHeaderPrefix.Length), entry.Value);
                }
            }

            // Required fields
            if (traceId == 0 || spanId == 0)
                return null;

            return new ZipkinSpanContext(traceId, spanId, parentId == 0 ? (ulong?)null : parentId, sampled, baggage);
        }
    }
}