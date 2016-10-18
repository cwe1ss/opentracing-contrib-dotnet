using System;
using System.Threading.Tasks;
using OpenTracing.Contrib.Testing;
using Xunit;

namespace OpenTracing.Contrib.Tests
{
    public class SpanContextAccessorTest
    {
        private ISpan CallSync(ISpanAccessor accessor)
        {
            return accessor.Span;
        }

        private async Task<ISpan> CallAsync(ISpanAccessor accessor)
        {
            await Task.Delay(15);
            return accessor.Span;
        }

        private ISpan GetSpan()
        {
            return new TestSpan(new TestTracer(), new TestSpanContext(), DateTime.UtcNow, "test", null, null);
        }

        [Fact]
        public void Returns_null_if_not_set()
        {
            var accessor = new SpanAccessor();

            Assert.Null(accessor.Span);
        }

        [Fact]
        public void Returns_context_in_sync_method()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Span = span;

            Assert.Same(span, accessor.Span);
        }

        [Fact]
        public void Returns_context_in_sync_call()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Span = span;

            var resultContext = CallSync(accessor);

            Assert.Same(span, resultContext);
        }

        [Fact]
        public async Task Returns_context_in_async_call()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Span = span;

            var resultContext = await CallAsync(accessor);

            Assert.Same(span, resultContext);
        }
    }
}