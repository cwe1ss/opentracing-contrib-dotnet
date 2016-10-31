using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenTracing.Instrumentation
{
    public class TraceContext : ITraceContext
    {
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
            private readonly ITraceContext _accessor;

            private bool _disposed;

            public PopWhenDisposed(ITraceContext accessor)
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