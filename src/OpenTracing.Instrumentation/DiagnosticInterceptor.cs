using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OpenTracing.Instrumentation
{
    public abstract class DiagnosticInterceptor : IDiagnosticInterceptor
    {
        private IDisposable _subscription;

        protected ILogger Logger { get; }
        protected ITracer Tracer { get; }
        protected ITraceContext TraceContext { get; }

        protected DiagnosticInterceptor(ILoggerFactory loggerFactory, ITracer tracer, ITraceContext traceContext)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (traceContext == null)
                throw new ArgumentNullException(nameof(traceContext));

            Logger = loggerFactory.CreateLogger(GetType());
            Tracer = tracer;
            TraceContext = traceContext;
        }

        public void Start()
        {
            _subscription = System.Diagnostics.DiagnosticListener.AllListeners.Subscribe(listener =>
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
    }
}