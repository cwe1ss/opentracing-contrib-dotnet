using System;
using System.Threading;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.EntityFrameworkCore.Proxies;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.EntityFrameworkCore
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

        public EntityFrameworkCoreInterceptor(ILoggerFactory loggerFactory, ITracer tracer)
            : base(loggerFactory, tracer)
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
            Execute(() =>
            {
                // TODO @cweiss !! OperationName ??
                string operationName = executeMethod;

                var span = Tracer.BuildSpan(operationName).Start();

                Tags.SpanKind.Set(span, Tags.SpanKindClient);
                Tags.Component.Set(span, Component);
                span.SetTag(TagCommandText, command.CommandText);

                span.SetTag(TagCommandText, command.CommandText);
                span.SetTag(TagMethod, executeMethod);
                span.SetTag(TagIsAsync, isAsync);;

                _span.Value = span;
            });
        }

        /// <summary>
        /// Microsoft.EntityFrameworkCore.Relational/Internal/RelationalDiagnostics.cs
        /// </summary>
        [DiagnosticName(EventAfterExecuteCommand)]
        public void OnAfterExecuteCommand(IDbCommand command, string executeMethod, bool isAsync)
        {
            Execute(() =>
            {
                ISpan span = _span.Value;
                if (span == null)
                {
                    Logger.LogError("Span not found");
                    return;
                }

                span.Finish();
            });
        }

        /// <summary>
        /// Microsoft.EntityFrameworkCore.Relational/Internal/RelationalDiagnostics.cs
        /// </summary>
        [DiagnosticName(EventCommandExecutionError)]
        public void OnCommandExecutionError(IDbCommand command, string executeMethod, bool isAsync, Exception exception)
        {
            Execute(() =>
            {
                ISpan span = _span.Value;
                if (span == null)
                {
                    Logger.LogError("Span not found");
                    return;
                }

                span.SetException(exception);
                span.Finish();
            });
        }
    }
}
