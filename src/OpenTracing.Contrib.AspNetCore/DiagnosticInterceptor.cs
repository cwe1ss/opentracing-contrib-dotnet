using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace OpenTracing.Contrib.AspNetCore
{
    public abstract class DiagnosticInterceptor : IDiagnosticInterceptor
    {
        private IDisposable _subscription;

        protected ILogger Logger { get; }
        protected ITracer Tracer { get; }

        protected DiagnosticInterceptor(ILoggerFactory loggerFactory, ITracer tracer)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            Logger = loggerFactory.CreateLogger(GetType());
            Tracer = tracer;
        }

        public void Start()
        {
            _subscription = DiagnosticListener.AllListeners.Subscribe(listener =>
            {
                listener.SubscribeWithAdapter(this, IsEnabled);
            });
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        protected virtual bool IsEnabled(string listenerName)
        {
            return true;
        }

        protected void Execute(Action action, [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace("{Event}-Start", callerMemberName);

                action();

                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace("{Event}-End", callerMemberName);
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "{Event} failed", callerMemberName);
            }
        }

        protected void DisposeActiveScope()
        {
            Execute(() =>
            {
                var scope = Tracer.ScopeManager.Active;
                if (scope == null)
                {
                    Logger.LogError("ActiveSpan not found");
                    return;
                }

                scope.Dispose();
            });
        }
    }
}
