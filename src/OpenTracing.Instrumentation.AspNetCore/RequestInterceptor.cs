using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Propagation;

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

        public RequestInterceptor(ILoggerFactory loggerFactory, ITracer tracer, ITraceContext spanAccessor)
            : base(loggerFactory, tracer, spanAccessor)
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
                var extractedSpanContext = TryExtractSpanContext(httpContext.Request);

                ISpan span = StartSpan(extractedSpanContext, httpContext.Request);

                // Push span to stack for in-process propagation.
                TraceContext.Push(span);
            });
        }

        [DiagnosticName(EventEndRequest)]
        public void OnEndRequest(HttpContext httpContext)
        {
            Execute(() =>
            {
                var span = PopAndFinishOrphaned();
                if (span == null)
                    return;

                span.SetTag(Tags.HttpStatusCode, httpContext.Response.StatusCode);
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
                ISpanContext spanContext = Tracer.Extract(Formats.HttpHeaders, new HeaderDictionaryCarrier(request.Headers));
                return spanContext;
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "Extracting SpanContext failed");
                return null;
            }
        }

        private ISpan StartSpan(ISpanContext extractedSpanContext, HttpRequest request)
        {
            var operationName = GetOperationName(request);

            var span = Tracer.BuildSpan(operationName)
                .AsChildOf(extractedSpanContext)
                .WithTag(Tags.Component, Component)
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag(Tags.HttpMethod, request.Method)
                .WithTag(Tags.HttpUrl, request.GetDisplayUrl())
                .WithTag(Tags.PeerHostname, request.Host.Host)
                .Start();

            return span;
        }

        private string GetOperationName(HttpRequest request)
        {
            // TODO @cweiss Make this configurable.
            return request.Path;
        }

        /// <summary>
        /// <para>This method will return the bottom-span from the stack.
        /// If there's more than one span on the stack, it will call <see cref="ISpan.Finish"/> on them.</para>
        /// <para>This should be called by outermost handlers who create root spans and want to finish all open spans.</para>
        /// </summary>
        private ISpan PopAndFinishOrphaned()
        {
            ISpan last = null;
            while (TraceContext.Count > 0)
            {
                last = TraceContext.TryPop();

                // if it hasn't been the last, it's orphaned and must be finished.
                if (TraceContext.Count > 0)
                {
                    Logger.LogError("Orphaned span detected. {SpanContext}", last.Context);
                    last.Finish();
                }
            }

            if (last == null)
            {
                Logger.LogError("Span not found");
            }

            return last;
        }

        private void HandleException(HttpContext httpContext, Exception exception)
        {
            try
            {
                var span = PopAndFinishOrphaned();
                if (span == null)
                    return;

                span.SetTag(Tags.HttpStatusCode, httpContext.Response.StatusCode);
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