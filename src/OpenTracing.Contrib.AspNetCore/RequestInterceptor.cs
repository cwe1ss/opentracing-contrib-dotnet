using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.AspNetCore
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

                var scope = Tracer.BuildSpan(operationName)
                    .AsChildOf(extractedSpanContext)
                    .WithTag(Tags.Component.Key, Component)
                    .WithTag(Tags.SpanKind.Key, Tags.SpanKindServer)
                    .WithTag(Tags.HttpMethod.Key, request.Method)
                    .WithTag(Tags.HttpUrl.Key, request.GetDisplayUrl())
                    .StartActive(finishSpanOnDispose: true);

                // Make sure the scope is disposed at the end of the request.
                httpContext.Response.RegisterForDispose(scope);
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
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "HandleException failed");
            }
        }
    }
}
