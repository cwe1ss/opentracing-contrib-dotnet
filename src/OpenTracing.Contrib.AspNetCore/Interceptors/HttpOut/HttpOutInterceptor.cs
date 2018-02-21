using System;
using System.Net.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing.Contrib.AspNetCore.Configuration;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.AspNetCore.Interceptors.HttpOut
{
    public class HttpOutInterceptor : DiagnosticInterceptor
    {
        // Diagnostic names:
        // https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/DiagnosticsHandlerLoggingStrings.cs
        public const string EventRequest = "System.Net.Http.Request";
        public const string EventResponse = "System.Net.Http.Response";
        public const string EventException = "System.Net.Http.Exception";

        public const string Component = "HttpHandler";

        public const string PropertySpan = "ot-span";

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

                ISpan span = Tracer.BuildSpan(operationName)
                    .WithTag(Tags.SpanKind.Key, Tags.SpanKindClient)
                    .WithTag(Tags.Component.Key, Component)
                    .WithTag(Tags.HttpMethod.Key, request.Method.ToString())
                    .WithTag(Tags.HttpUrl.Key, request.RequestUri.ToString())
                    .WithTag(Tags.PeerHostname.Key, request.RequestUri.Host)
                    .WithTag(Tags.PeerPort.Key, request.RequestUri.Port)
                    .Start();

                _options.OnRequest?.Invoke(span, request);

                Tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new HttpHeadersCarrier(request.Headers));

                request.Properties[PropertySpan] = span;
            });
        }

        [DiagnosticName(EventException)]
        public void OnException(HttpRequestMessage request, Exception ex)
        {
            Execute(() =>
            {
                if (TryGetSpanFromRequestProperties(request, out ISpan span))
                {
                    span.SetException(ex);
                }
            });
        }

        [DiagnosticName(EventResponse)]
        public void OnResponse(HttpResponseMessage response)
        {
            Execute(() =>
            {
                if (TryGetSpanFromRequestProperties(response.RequestMessage, out ISpan span))
                {
                    Tags.HttpStatus.Set(span, (int)response.StatusCode);
                    span.Finish();
                }
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

        private bool TryGetSpanFromRequestProperties(HttpRequestMessage request, out ISpan span)
        {
            request.Properties.TryGetValue(PropertySpan, out object objSpan);
            span = objSpan as ISpan;
            return span != null;
        }
    }
}
