using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.Http
{
    /// <summary>
    /// Creates a span and propagates the context via HTTP headers to the target.
    /// </summary>
    public class OpenTracingDelegatingHandler : DelegatingHandler
    {
        private const string Component = "HttpClient";

        // Poor man's singleton dependency injection :)
        private static IOutgoingHttpOperationName DefaultOperationName = new DefaultOutgoingHttpOperationName();

        private readonly ITracer _tracer;
        private readonly ISpanAccessor _spanAccessor;
        private readonly IOutgoingHttpOperationName _operationName;

        public OpenTracingDelegatingHandler(ITracer tracer, ISpanAccessor spanAccessor)
            : this(tracer, spanAccessor, DefaultOperationName)
        {
        }

        public OpenTracingDelegatingHandler(
            ITracer tracer,
            ISpanAccessor spanAccessor,
            IOutgoingHttpOperationName operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (spanAccessor == null)
                throw new ArgumentNullException(nameof(spanAccessor));

            if (operationName == null)
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            _spanAccessor = spanAccessor;
            _operationName = operationName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (ISpan span = StartSpan(request))
            {
                TryInject(span, request);

                try
                {
                    var response = await base.SendAsync(request, cancellationToken);

                    span.SetTag(Tags.HttpStatusCode, (int)response.StatusCode);

                    return response;
                }
                catch (Exception ex)
                {
                    span.SetException(ex);
                    throw;
                }
            }
        }

        private ISpan StartSpan(HttpRequestMessage request)
        {
            // A new trace will be started if this is null.
            ISpan parent = _spanAccessor.CurrentSpan;

            string operationName = _operationName.GetOperationName(request);

            ISpan span = _tracer.BuildSpan(operationName)
                .AsChildOf(parent)
                .WithTag(Tags.SpanKind, Tags.SpanKindClient)
                .WithTag(Tags.Component, Component)
                .WithTag(Tags.HttpMethod, request.Method.ToString())
                .WithTag(Tags.HttpUrl, request.RequestUri.ToString())
                .WithTag(Tags.PeerHostname, request.RequestUri.Host)
                .WithTag(Tags.PeerPort, request.RequestUri.Port)
                .Start();

            return span;
        }

        /// <summary>
        /// Fail-safe inject. The HTTP call shouldn't fail if there's a problem with the tracer.
        /// </summary>
        private void TryInject(ISpan span, HttpRequestMessage request)
        {
            try
            {
                _tracer.Inject(span.Context, Formats.HttpHeaders, new HttpHeadersCarrier(request.Headers));
            }
            catch (Exception ex)
            {
                span.SetException(ex, "Inject failed");
            }
        }
    }
}