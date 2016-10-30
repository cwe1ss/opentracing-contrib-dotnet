using System.Threading.Tasks;
using OpenTracing.Contrib.Testing;
using Xunit;

namespace OpenTracing.Contrib.Tests
{
    public class SpanContextAccessorTest
    {
        private ISpan CallSync(ISpanAccessor accessor)
        {
            return accessor.CurrentSpan;
        }

        private async Task<ISpan> CallAsync(ISpanAccessor accessor)
        {
            await Task.Delay(10);
            return accessor.CurrentSpan;
        }

        private async Task Delay()
        {
            await Task.Delay(10);
        }

        private ISpan GetSpan()
        {
            return new TestSpan(new TestTracer(), new TestSpanContext(), "test", null);
        }

        [Fact]
        public void Returns_span_in_sync_method()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Push(span);

            Assert.Same(span, accessor.CurrentSpan);
        }

        [Fact]
        public void Returns_span_in_sync_call()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Push(span);

            var resultContext = CallSync(accessor);

            Assert.Same(span, resultContext);
        }

        [Fact]
        public async Task Returns_span_in_async_call()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Push(span);

            var resultContext = await CallAsync(accessor);

            Assert.Same(span, resultContext);
        }

        [Fact]
        public void Pop_succeeds_if_stack_empty()
        {
            var accessor = new SpanAccessor();

            Assert.Null(accessor.TryPop());
        }

        [Fact]
        public void CurrentSpan_succeeds_if_stack_empty()
        {
            var accessor = new SpanAccessor();

            Assert.Null(accessor.CurrentSpan);
        }

        [Fact]
        public void Count_succeeds_if_stack_empty()
        {
            var accessor = new SpanAccessor();

            Assert.Equal(0, accessor.Count);
        }

        [Fact]
        public void CurrentSpan_keeps_span_on_stack()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            accessor.Push(span);

            Assert.Equal(span, accessor.CurrentSpan);
            Assert.Equal(1, accessor.Count);
        }

        [Fact]
        public void Using_pops_span_from_stack()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            using (accessor.Push(span))
            {
                Assert.Equal(span, accessor.CurrentSpan);
            }

            Assert.Null(accessor.CurrentSpan);
        }

        [Fact]
        public async Task Async_Using_pops_span_from_stack()
        {
            var accessor = new SpanAccessor();
            var span = GetSpan();

            using (accessor.Push(span))
            {
                await Delay();
                Assert.Equal(span, accessor.CurrentSpan);
            }

            await Delay();
            Assert.Null(accessor.CurrentSpan);
        }

        [Fact]
        public void Sync_CurrentSpan_for_nested_spans_succeeds_with_using()
        {
            var accessor = new SpanAccessor();
            var span1 = GetSpan();
            var span2 = GetSpan();

            using (accessor.Push(span1))
            {
                Assert.Equal(span1, accessor.CurrentSpan);

                using (accessor.Push(span2))
                {
                    Assert.Equal(span2, accessor.CurrentSpan);
                }

                Assert.Equal(span1, accessor.CurrentSpan);
            }

            Assert.Null(accessor.CurrentSpan);
        }

        [Fact]
        public async Task Async_CurrentSpan_for_nested_spans_succeeds_with_using()
        {
            var accessor = new SpanAccessor();
            var span1 = GetSpan();
            var span2 = GetSpan();
            var span3 = GetSpan();

            using (accessor.Push(span1))
            {
                await Delay();
                Assert.Equal(span1, accessor.CurrentSpan);

                await Delay();
                using (accessor.Push(span2))
                {
                    await Delay();
                    Assert.Equal(span2, accessor.CurrentSpan);

                    using (accessor.Push(span3))
                    {
                        await Delay();
                        Assert.Equal(span3, accessor.CurrentSpan);
                    }

                    await Delay();
                    Assert.Equal(span2, accessor.CurrentSpan);
                }

                await Delay();
                Assert.Equal(span1, accessor.CurrentSpan);
            }

            await Delay();
            Assert.Null(accessor.CurrentSpan);
        }

        public void Sync_CurrentSpan_for_nested_spans_succeeds_with_pop()
        {
            var accessor = new SpanAccessor();
            var span1 = GetSpan();
            var span2 = GetSpan();

            accessor.Push(span1);
            Assert.Equal(span1, accessor.CurrentSpan);

            accessor.Push(span2);
            Assert.Equal(span2, accessor.CurrentSpan);
            accessor.TryPop();

            Assert.Equal(span1, accessor.CurrentSpan);
            accessor.TryPop();

            Assert.Null(accessor.CurrentSpan);
        }

        [Fact]
        public async Task Async_CurrentSpan_for_nested_spans_succeeds_with_pop()
        {
            var accessor = new SpanAccessor();
            var span1 = GetSpan();
            var span2 = GetSpan();
            var span3 = GetSpan();

            accessor.Push(span1);
            await Delay();

            Assert.Equal(span1, accessor.CurrentSpan);

            await Delay();
            accessor.Push(span2);
            await Delay();

            Assert.Equal(span2, accessor.CurrentSpan);

            accessor.Push(span3);
            await Delay();

            Assert.Equal(span3, accessor.CurrentSpan);

            await Delay();
            accessor.TryPop();

            Assert.Equal(span2, accessor.CurrentSpan);

            accessor.TryPop();
            await Delay();

            Assert.Equal(span1, accessor.CurrentSpan);

            accessor.TryPop();
            await Delay();

            Assert.Null(accessor.CurrentSpan);
        }
    }
}