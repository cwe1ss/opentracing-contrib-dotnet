using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.AspNetCore
{
    public class OpenTracingMiddleware
    {
        private const string Component = "AspNetCore";
        private const string HeaderSampling = "ot-debug";

        private readonly RequestDelegate _next;
        private readonly ITracer _tracer;
        private readonly IIncomingHttpOperationName _operationName;
        private readonly ISpanContextAccessor _spanContextAccessor;
        private readonly ILogger _logger;

        public OpenTracingMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            ITracer tracer,
            IIncomingHttpOperationName operationName,
            ISpanContextAccessor spanContextAccessor = null)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(next));

            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (operationName == null)
                throw new ArgumentNullException(nameof(operationName));

            _next = next;
            _tracer = tracer;
            _operationName = operationName;
            _spanContextAccessor = spanContextAccessor;
            _logger = loggerFactory.CreateLogger<OpenTracingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var extractedSpanContext = TryExtractSpanContext(context.Request);

            ISpan span = null;
            try
            {
                span = StartSpan(extractedSpanContext, context.Request);

                // Save span context for in-process propagation.
                context.Items[typeof(ISpanContext)] = span.Context;
                if (_spanContextAccessor != null)
                {
                    _spanContextAccessor.SpanContext = span.Context;
                }

                await _next(context);

                span.SetTag(Tags.HttpStatusCode, context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                span?.SetException(ex);
                throw;
            }
            finally
            {
                span?.Dispose();
            }
        }

        private ISpanContext TryExtractSpanContext(HttpRequest request)
        {
            try
            {
                ISpanContext spanContext = _tracer.Extract(Formats.HttpHeaders, new HeaderDictionaryCarrier(request.Headers));
                return spanContext;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(0, ex, "Extracting SpanContext failed");
                return null;
            }
        }

        private ISpan StartSpan(ISpanContext extractedSpanContext, HttpRequest request)
        {
            var operationName = _operationName.GetOperationName(request);

            var span = _tracer.BuildSpan(operationName)
                .AsChildOf(extractedSpanContext)
                .WithTag(Tags.Component, Component)
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag(Tags.HttpMethod, request.Method)
                .WithTag(Tags.HttpUrl, request.GetDisplayUrl())
                .WithTag(Tags.SamplingPriority, GetSamplingPriority(request))
                .Start();

            return span;
        }

        private int GetSamplingPriority(HttpRequest request)
        {
            StringValues debugHeader;
            if (!request.Headers.TryGetValue(HeaderSampling, out debugHeader))
                return 0;

            string debugHeaderString = debugHeader.ToString();

            if (debugHeaderString.Equals("1") || debugHeaderString.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}