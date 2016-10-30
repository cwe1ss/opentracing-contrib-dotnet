using System;
using System.Collections.Generic;
#if NET451
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
        private static readonly string LogicalDataKey = "__SpanStack__" + AppDomain.CurrentDomain.Id;

        private Stack<ISpan> GetSpanStack()
        {
            var handle = CallContext.LogicalGetData(LogicalDataKey) as ObjectHandle;
            var spanStack = handle?.Unwrap() as Stack<ISpan>;

            if (spanStack == null)
            {
                spanStack = new Stack<ISpan>();
                CallContext.LogicalSetData(LogicalDataKey, new ObjectHandle(spanStack));
            }

            return spanStack;
        }

#elif NETSTANDARD1_3
        private AsyncLocal<Stack<ISpan>> _asyncLocalSpanStack = new AsyncLocal<Stack<ISpan>>();

        private Stack<ISpan> GetSpanStack()
        {
            var spanStack = _asyncLocalSpanStack.Value;

            if (spanStack == null)
            {
                spanStack = new Stack<ISpan>();
                _asyncLocalSpanStack.Value = spanStack;
            }

            return spanStack;
        }
#endif

        public int Count => GetSpanStack().Count;

        public bool IsEmpty() => Count == 0;

        public ISpan CurrentSpan
        {
            get
            {
                Stack<ISpan> stack = GetSpanStack();

                if (stack.Count == 0)
                    return null;

                return stack.Peek();
            }
        }

        public IDisposable Push(ISpan span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            Stack<ISpan> stack = GetSpanStack();
            stack.Push(span);

            return new PopWhenDisposed(this);
        }

        public ISpan TryPop()
        {
            Stack<ISpan> stack = GetSpanStack();

            if (stack.Count == 0)
                return null;

            return stack.Pop();
        }

        private class PopWhenDisposed : IDisposable
        {
            private readonly ISpanAccessor _accessor;

            private bool _disposed;

            public PopWhenDisposed(ISpanAccessor accessor)
            {
                _accessor = accessor;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _accessor.TryPop();
                _disposed = true;
            }
        }
    }
}