using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.Core
{
    public class Instrumentor : IInstrumentor
    {
        private readonly IEnumerable<IDiagnosticInterceptor> _interceptors;

        private bool _started;
        private bool _disposed;

        public Instrumentor(IEnumerable<IDiagnosticInterceptor> interceptors)
        {
            _interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));
        }

        public void Start()
        {
            if (_started)
                return;

            foreach (var interceptor in _interceptors)
            {
                interceptor.Start();
            }

            _started = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_interceptors != null)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
