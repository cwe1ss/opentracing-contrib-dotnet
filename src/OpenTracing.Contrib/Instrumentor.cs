using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib
{
    public class Instrumentor : IInstrumentor
    {
        private readonly IEnumerable<IDiagnosticInterceptor> _interceptors;

        private bool _started;
        private bool _disposed;

        public Instrumentor(IEnumerable<IDiagnosticInterceptor> interceptors)
        {
            if (interceptors == null)
                throw new ArgumentNullException(nameof(interceptors));

            _interceptors = interceptors;
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
