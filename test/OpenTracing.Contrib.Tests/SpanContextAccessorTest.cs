using System.Threading.Tasks;
using OpenTracing.Contrib.Testing;
using Xunit;

namespace OpenTracing.Contrib.Tests
{
    public class SpanContextAccessorTest
    {
        private ISpanContext CallSync(ISpanContextAccessor accessor)
        {
            return accessor.SpanContext;
        }

        private async Task<ISpanContext> CallAsync(ISpanContextAccessor accessor)
        {
            await Task.Delay(15);
            return accessor.SpanContext;
        }

        [Fact]
        public void Returns_null_if_not_set()
        {
            var accessor = new SpanContextAccessor();

            Assert.Null(accessor.SpanContext);
        }

        [Fact]
        public void Returns_context_in_sync_method()
        {
            var accessor = new SpanContextAccessor();
            var spanContext = new TestSpanContext();

            accessor.SpanContext = spanContext;

            Assert.Same(spanContext, accessor.SpanContext);
        }

        [Fact]
        public void Returns_context_in_sync_call()
        {
            var accessor = new SpanContextAccessor();
            var spanContext = new TestSpanContext();

            accessor.SpanContext = spanContext;

            var resultContext = CallSync(accessor);

            Assert.Same(spanContext, resultContext);
        }

        [Fact]
        public async Task Returns_context_in_async_call()
        {
            var accessor = new SpanContextAccessor();
            var spanContext = new TestSpanContext();

            accessor.SpanContext = spanContext;

            var resultContext = await CallAsync(accessor);

            Assert.Same(spanContext, resultContext);
        }
    }
}