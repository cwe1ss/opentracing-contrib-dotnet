using System;
using System.Net.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing.Contrib.Core.Configuration;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Core.Interceptors.HttpOut
{
    public class HttpOutInterceptor : DiagnosticInterceptor
    {
        // Diagnostic names:
        // https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/DiagnosticsHandlerLoggingStrings.cs
        public const string EventRequest = "System.Net.Http.Request";
        public const string EventResponse = "System.Net.Http.Response";
        public const string EventException = "System.Net.Http.Exception";

        public const string Component = "HttpHandler";

        private readonly HttpOutOptions _options;

        public HttpOutInterceptor(ILoggerFactory loggerFactory, ITracer tracer, IOptions<HttpOutOptions> options)
            : base(loggerFactory, tracer)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override bool IsEnabled(string listenerName)
        {
            if (listenerName == EventRequest) return true;
            if (listenerName == EventResponse) return true;
            if (listenerName == EventException) return true;

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

                string operationName = _options.OperationNameResolver(request);

                IScope scope = Tracer.BuildSpan(operationName)
                    .WithTag(Tags.SpanKind.Key, Tags.SpanKindClient)
                    .WithTag(Tags.Component.Key, Component)
                    .WithTag(Tags.HttpMethod.Key, request.Method.ToString())
                    .WithTag(Tags.HttpUrl.Key, request.RequestUri.ToString())
                    .WithTag(Tags.PeerHostname.Key, request.RequestUri.Host)
                    .WithTag(Tags.PeerPort.Key, request.RequestUri.Port)
                    .StartActive(finishSpanOnDispose: true);

                _options.OnRequest?.Invoke(scope.Span, request);

                Tracer.Inject(scope.Span.Context, BuiltinFormats.HttpHeaders, new HttpHeadersCarrier(request.Headers));
            });
        }

        [DiagnosticName(EventException)]
        public void OnException(HttpRequestMessage request, Exception exception)
        {
            Execute(() =>
            {
                Tracer.ActiveSpan?.SetException(exception);
            });
        }

        [DiagnosticName(EventResponse)]
        public void OnResponse(HttpResponseMessage response)
        {
            Execute(() =>
            {
                IScope scope = Tracer.ScopeManager.Active;

                if (response != null)
                {
                    scope?.Span?.SetTag(Tags.HttpStatus.Key, (int)response.StatusCode);
                }

                scope?.Dispose();
             });
        }

        private bool ShouldIgnore(HttpRequestMessage request)
        {
            foreach (Func<HttpRequestMessage, bool> shouldIgnore in _options.ShouldIgnore)
            {
                if (shouldIgnore(request))
                    return true;
            }

            return false;
        }
    }
}
