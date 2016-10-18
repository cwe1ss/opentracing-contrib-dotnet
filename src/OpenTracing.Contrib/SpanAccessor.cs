#if NET451
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#elif NETSTANDARD1_3
using System.Threading;
#endif

namespace OpenTracing.Contrib
{
    public class SpanAccessor : ISpanAccessor
    {
#if NET451
        private static readonly string LogicalDataKey = "__ISpan_Current__" + AppDomain.CurrentDomain.Id;

        public ISpan Span
        {
            get
            {
                var handle = CallContext.LogicalGetData(LogicalDataKey) as ObjectHandle;
                return handle?.Unwrap() as ISpan;
            }
            set
            {
                CallContext.LogicalSetData(LogicalDataKey, new ObjectHandle(value));
            }
        }

#elif NETSTANDARD1_3
        private AsyncLocal<ISpan> _currentSpan = new AsyncLocal<ISpan>();
        public ISpan Span
        {
            get
            {
                return _currentSpan.Value;
            }
            set
            {
                _currentSpan.Value = value;
            }
        }
#endif
    }
}