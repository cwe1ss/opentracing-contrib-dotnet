using System;
using System.Threading;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Instrumentation.EntityFrameworkCore.Proxies;

namespace OpenTracing.Instrumentation.EntityFrameworkCore
{
    public class EntityFrameworkCoreInterceptor : DiagnosticInterceptor
    {
        private const string EventBeforeExecuteCommand = "Microsoft.EntityFrameworkCore.BeforeExecuteCommand";
        private const string EventAfterExecuteCommand = "Microsoft.EntityFrameworkCore.AfterExecuteCommand";
        private const string EventCommandExecutionError = "Microsoft.EntityFrameworkCore.CommandExecutionError";

        private const string Component = "EFCore";

        private const string TagCommandText = "ef.command";
        private const string TagMethod = "ef.method";
        private const string TagIsAsync = "ef.async";


        private AsyncLocal<ISpan> _span = new AsyncLocal<ISpan>();

        public EntityFrameworkCoreInterceptor(ILoggerFactory loggerFactory, ITracer tracer, ITraceContext traceContext)
            : base(loggerFactory, tracer, traceContext)
        {
        }

        protected override bool IsEnabled(string listenerName)
        {
            if (listenerName == EventBeforeExecuteCommand) return true;
            if (listenerName == EventAfterExecuteCommand) return true;
            if (listenerName == EventCommandExecutionError) return true;

            return false;
        }

        /// <summary>
        /// Microsoft.EntityFrameworkCore.Relational/Internal/RelationalDiagnostics.cs
        /// </summary>
        [DiagnosticName(EventBeforeExecuteCommand)]
        public void OnBeforeExecuteCommand(IDbCommand command, string executeMethod, bool isAsync)
        {
            try
            {
                var parent = TraceContext.CurrentSpan;
                if (parent == null)
                {
                    Logger.LogDebug("No parent span found. Creating new span.");
                }

                // TODO @cweiss !! OperationName ??
                string operationName = executeMethod;

                var span = Tracer.BuildSpan(operationName)
                    .AsChildOf(parent)
                    .WithTag(Tags.SpanKind, Tags.SpanKindClient)
                    .WithTag(Tags.Component, Component)
                    .WithTag(TagCommandText, command.CommandText)
                    .WithTag(TagMethod, executeMethod)
                    .WithTag(TagIsAsync, isAsync)
                    .Start();

                _span.Value = span;
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "OnBeforeExecuteCommand failed");
            }
        }

        /// <summary>
        /// Microsoft.EntityFrameworkCore.Relational/Internal/RelationalDiagnostics.cs
        /// </summary>
        [DiagnosticName(EventAfterExecuteCommand)]
        public void OnAfterExecuteCommand(IDbCommand command, string executeMethod, bool isAsync)
        {
            try
            {
                ISpan span = _span.Value;
                if (span == null)
                {
                    Logger.LogError("Span not found");
                    return;
                }

                span.Finish();
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "OnAfterExecuteCommand failed");
            }
        }

        /// <summary>
        /// Microsoft.EntityFrameworkCore.Relational/Internal/RelationalDiagnostics.cs
        /// </summary>
        [DiagnosticName(EventCommandExecutionError)]
        public void OnCommandExecutionError(IDbCommand command, string executeMethod, bool isAsync, Exception exception)
        {
            try
            {
                ISpan span = _span.Value;
                if (span == null)
                {
                    Logger.LogError("Span not found");
                    return;
                }

                span.SetException(exception);
                span.Finish();
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "OnAfterExecuteCommand failed");
            }
        }
    }
}