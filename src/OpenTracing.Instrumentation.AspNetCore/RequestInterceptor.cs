using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Instrumentation.AspNetCore
{
    public class RequestInterceptor : DiagnosticInterceptor
    {
        // Events from:
        // - Microsoft.AspNetCore.Hosting -> HostingApplication,
        // - Microsoft.AspNetCore.Diagnostics -> ExceptionHandlerMiddleware,
        // - Microsoft.AspNetCore.Diagnostics -> DeveloperExceptionPageMiddleware
        private const string EventBeginRequest = "Microsoft.AspNetCore.Hosting.BeginRequest";
        private const string EventEndRequest = "Microsoft.AspNetCore.Hosting.EndRequest";
        private const string EventHostingUnhandledException = "Microsoft.AspNetCore.Hosting.UnhandledException";
        private const string EventDiagnosticsHandledException = "Microsoft.AspNetCore.Diagnostics.HandledException";
        private const string EventDiagnosticsUnhandledException = "Microsoft.AspNetCore.Diagnostics.UnhandledException";

        private const string Component = "AspNetCore";

        public RequestInterceptor(ILoggerFactory loggerFactory, ITracer tracer)
            : base(loggerFactory, tracer)
        {
        }

        protected override bool IsEnabled(string listenerName)
        {
            if (listenerName == EventBeginRequest) return true;
            if (listenerName == EventEndRequest) return true;
            if (listenerName == EventHostingUnhandledException) return true;
            if (listenerName == EventDiagnosticsHandledException) return true;
            if (listenerName == EventDiagnosticsUnhandledException) return true;

            return false;
        }

        [DiagnosticName(EventBeginRequest)]
        public void OnBeginRequest(HttpContext httpContext)
        {
            Execute(() =>
            {
                var request = httpContext.Request;

                var extractedSpanContext = TryExtractSpanContext(request);

                var operationName = GetOperationName(request);

                var span = Tracer.BuildSpan(operationName)
                    .AsChildOf(extractedSpanContext)
                    .Start();

                Tags.Component.Set(span, Component);
                Tags.SpanKind.Set(span, Tags.SpanKindServer);
                Tags.HttpMethod.Set(span, request.Method);
                Tags.HttpUrl.Set(span, request.GetDisplayUrl());
                Tags.PeerHostname.Set(span, request.Host.Host);
            });
        }

        [DiagnosticName(EventEndRequest)]
        public void OnEndRequest(HttpContext httpContext)
        {
            Execute(() =>
            {
                var span = Tracer.ActiveSpan;
                if (span == null)
                {
                    Logger.LogError("ActiveSpan not found");
                    return;
                }

                Tags.HttpStatus.Set(span, httpContext.Response.StatusCode);
                span.Finish();
            });
        }

        [DiagnosticName(EventHostingUnhandledException)]
        public void OnHostingUnhandledException(HttpContext httpContext, Exception exception)
        {
            HandleException(httpContext, exception);
        }

        [DiagnosticName(EventDiagnosticsHandledException)]
        public void OnDiagnosticsHandledException(HttpContext httpContext, Exception exception)
        {
            HandleException(httpContext, exception);
        }

        [DiagnosticName(EventDiagnosticsUnhandledException)]
        public void OnDiagnosticsUnhandledException(HttpContext httpContext, Exception exception)
        {
            HandleException(httpContext, exception);
        }

        private ISpanContext TryExtractSpanContext(HttpRequest request)
        {
            try
            {
                ISpanContext spanContext = Tracer.Extract(BuiltinFormats.HttpHeaders, new HeaderDictionaryCarrier(request.Headers));
                return spanContext;
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "Extracting SpanContext failed");
                return null;
            }
        }

        private string GetOperationName(HttpRequest request)
        {
            // TODO @cweiss Make this configurable.
            return request.Path;
        }

        private void HandleException(HttpContext httpContext, Exception exception)
        {
            try
            {
                var span = Tracer.ActiveSpan;
                if (span == null)
                {
                    Logger.LogError("ActiveSpan not found");
                    return;
                }

                Tags.HttpStatus.Set(span, httpContext.Response.StatusCode);
                span.SetException(exception);
                span.Finish();
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "HandleException failed");
            }
        }
    }
}
