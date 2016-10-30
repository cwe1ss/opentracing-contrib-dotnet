using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTracing.Contrib.TracerAbstractions;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.ZipkinTracer.Propagation
{
    public class TextMapPropagator : IPropagator
    {
        private const string IdFormat = "x4";

        // https://github.com/openzipkin/zipkin-csharp/blob/master/src/Zipkin.Tracer/TraceHeader.cs
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
                throw new InvalidOperationException($"Carrier must be a '{nameof(ITextMap)}'");

            // TODO @cweiss !!! ToString("x4"), validation, ...

            textMap.Set(TraceIdHeader, context.TraceId.ToString(IdFormat));
            textMap.Set(SpanIdHeader, context.SpanId.ToString(IdFormat));
            textMap.Set(SampledHeader, context.Sampled ? SampledTrue : SampledFalse);

            // TODO do we have to send parent id??
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
                throw new InvalidOperationException($"Carrier must be a '{nameof(ITextMap)}'");

            ulong traceId = 0, spanId = 0, parentId = 0;
            bool sampled = false;
            Dictionary<string, string> baggage = null;

            foreach (var entry in textMap.GetEntries())
            {
                if (entry.Key == TraceIdHeader)
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out traceId);
                }
                else if (entry.Key == SpanIdHeader)
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out spanId);
                }
                else if (entry.Key == ParentIdHeader)
                {
                    ulong.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parentId);
                }
                else if (entry.Key == SampledHeader && entry.Value == SampledTrue)
                {
                    sampled = true;
                }
                else if (entry.Key.StartsWith(BaggageHeaderPrefix))
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