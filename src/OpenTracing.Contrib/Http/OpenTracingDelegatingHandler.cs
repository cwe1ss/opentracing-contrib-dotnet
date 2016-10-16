using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.Http
{
    /// <summary>
    /// Creates a child span if there is an active parent span and propagates the context
    /// via HTTP headers to the target.
    /// </summary>
    public class OpenTracingDelegatingHandler : DelegatingHandler
    {
        private const string Component = "HttpClient";

        // Poor man's singleton dependency injection :)
        private static IHttpOperationName DefaultOperationName = new DefaultHttpOperationName();

        private readonly ITracer _tracer;
        private readonly ISpanContextAccessor _spanContextAccessor;
        private readonly IHttpOperationName _operationName;

        public OpenTracingDelegatingHandler(ITracer tracer, ISpanContextAccessor spanContextAccessor)
            : this(tracer, spanContextAccessor, DefaultOperationName)
        {
        }

        public OpenTracingDelegatingHandler(
            ITracer tracer,
            ISpanContextAccessor spanContextAccessor,
            IHttpOperationName operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (spanContextAccessor == null)
                throw new ArgumentNullException(nameof(spanContextAccessor));

            if (operationName == null)
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            _spanContextAccessor = spanContextAccessor;
            _operationName = operationName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ISpan childSpan = null;

            try
            {
                childSpan = StartChildSpan(request);

                // Propagate span context via HTTP request headers.
                if (childSpan != null)
                {
                    _tracer.Inject(childSpan.Context, Formats.HttpHeaders, new HttpHeadersCarrier(request.Headers));
                }

                var response = await base.SendAsync(request, cancellationToken);

                childSpan?.SetTag(Tags.HttpStatusCode, (int)response.StatusCode);

                return response;
            }
            catch (Exception ex)
            {
                childSpan?.SetException(ex);
                throw;
            }
            finally
            {
                childSpan?.Dispose();
            }
        }

        private ISpan StartChildSpan(HttpRequestMessage request)
        {
            // A http request is always considered to be a child of some parent operation.
            // If there's no parent, we don't start a new span.

            ISpanContext parent = _spanContextAccessor.SpanContext;
            if (parent == null)
                return null;

            // Start a child span.

            string operationName = _operationName.GetOperationName(request);

            ISpan childSpan = _tracer.BuildSpan(operationName)
                .AsChildOf(parent)
                .WithTag(Tags.SpanKind, Tags.SpanKindClient)
                .WithTag(Tags.Component, Component)
                .WithTag(Tags.HttpMethod, request.Method.ToString())
                .WithTag(Tags.HttpUrl, request.RequestUri.ToString())
                .Start();

            return childSpan;
        }
    }
}