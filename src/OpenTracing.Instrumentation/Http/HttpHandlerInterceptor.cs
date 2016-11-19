using System;
using System.Net.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Propagation;

namespace OpenTracing.Instrumentation.Http
{
    public class HttpHandlerInterceptor : DiagnosticInterceptor
    {
        // Diagnostic names:
        // https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Net/Http/HttpHandlerLoggingStrings.cs
        private const string EventRequest = "System.Net.Http.Request";
        private const string EventResponse = "System.Net.Http.Response";

        private const string Component = "HttpHandler";


        private const string PropertyIgnore = "ot-ignore";
        private const string PropertySpan = "ot-span";


        public HttpHandlerInterceptor(ILoggerFactory loggerFactory, ITracer tracer, ITraceContext traceContext)
            : base(loggerFactory, tracer, traceContext)
        {
        }

        protected override bool IsEnabled(string listenerName)
        {
            if (listenerName == EventRequest) return true;
            if (listenerName == EventResponse) return true;

            return false;
        }

        [DiagnosticName(EventRequest)]
        public void OnRequest(HttpRequestMessage request)
        {
            Execute(() =>
            {
                if (ShouldIgnore(request))
                {
                    Logger.LogDebug("Ignoring Request {RequestUri}", request.RequestUri);
                    return;
                }

                var span = StartSpan(request);

                Tracer.Inject(span.Context, Formats.HttpHeaders, new HttpHeadersCarrier(request.Headers));

                request.Properties[PropertySpan] = span;
            });
        }

        [DiagnosticName(EventResponse)]
        public void OnResponse(HttpResponseMessage response)
        {
            Execute(() =>
            {
                if (response.RequestMessage.Properties.ContainsKey(PropertyIgnore))
                    return;

                object objSpan;
                response.RequestMessage.Properties.TryGetValue(PropertySpan, out objSpan);
                ISpan span = objSpan as ISpan;

                if (span == null)
                {
                    Logger.LogError("Span not found in RequestMessage");
                    return;
                }

                span.SetTag(Tags.HttpStatusCode, (int)response.StatusCode);
                span.Finish();
            });
        }

        private bool ShouldIgnore(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(PropertyIgnore))
                return true;

            // TODO @cweiss other hooks?

            return false;
        }

        private ISpan StartSpan(HttpRequestMessage request)
        {
            ISpan parent = TraceContext.CurrentSpan;

            if (parent == null)
            {
                Logger.LogDebug("No parent span found. Creating new span.");
            }

            string operationName = GetOperationName(request);

            ISpan span = Tracer.BuildSpan(operationName)
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

        private string GetOperationName(HttpRequestMessage request)
        {
            // TODO @cweiss make this configurable

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return request.Method.Method + "_" + request.RequestUri.AbsolutePath.TrimStart('/');
        }
    }
}