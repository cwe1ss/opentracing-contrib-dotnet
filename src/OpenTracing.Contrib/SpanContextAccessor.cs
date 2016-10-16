#if NET451
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#elif NETSTANDARD1_3
using System.Threading;
#endif

namespace OpenTracing.Contrib
{
    public class SpanContextAccessor : ISpanContextAccessor
    {
#if NET451
        private static readonly string LogicalDataKey = "__ISpanContext_Current__" + AppDomain.CurrentDomain.Id;

        public ISpanContext SpanContext
        {
            get
            {
                var handle = CallContext.LogicalGetData(LogicalDataKey) as ObjectHandle;
                return handle?.Unwrap() as ISpanContext;
            }
            set
            {
                CallContext.LogicalSetData(LogicalDataKey, new ObjectHandle(value));
            }
        }

#elif NETSTANDARD1_3
        private AsyncLocal<ISpanContext> _currentSpanContext = new AsyncLocal<ISpanContext>();
        public ISpanContext SpanContext
        {
            get
            {
                return _currentSpanContext.Value;
            }
            set
            {
                _currentSpanContext.Value = value;
            }
        }
#endif
    }
}