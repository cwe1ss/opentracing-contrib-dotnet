using System.Threading.Tasks;
using OpenTracing.Instrumentation;
using OpenTracing.Testing;
using Xunit;

namespace OpenTracing.Tests
{
    public class SpanContextAccessorTest
    {
        private ISpan CallSync(ITraceContext traceContext)
        {
            return traceContext.CurrentSpan;
        }

        private async Task<ISpan> CallAsync(ITraceContext traceContext)
        {
            await Task.Delay(10);
            return traceContext.CurrentSpan;
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
            var traceContext = new TraceContext();
            var span = GetSpan();

            traceContext.Push(span);

            Assert.Same(span, traceContext.CurrentSpan);
        }

        [Fact]
        public void Returns_span_in_sync_call()
        {
            var traceContext = new TraceContext();
            var span = GetSpan();

            traceContext.Push(span);

            var resultContext = CallSync(traceContext);

            Assert.Same(span, resultContext);
        }

        [Fact]
        public async Task Returns_span_in_async_call()
        {
            var traceContext = new TraceContext();
            var span = GetSpan();

            traceContext.Push(span);

            var resultContext = await CallAsync(traceContext);

            Assert.Same(span, resultContext);
        }

        [Fact]
        public void Pop_succeeds_if_stack_empty()
        {
            var traceContext = new TraceContext();

            Assert.Null(traceContext.TryPop());
        }

        [Fact]
        public void CurrentSpan_succeeds_if_stack_empty()
        {
            var traceContext = new TraceContext();

            Assert.Null(traceContext.CurrentSpan);
        }

        [Fact]
        public void Count_succeeds_if_stack_empty()
        {
            var traceContext = new TraceContext();

            Assert.Equal(0, traceContext.Count);
        }

        [Fact]
        public void CurrentSpan_keeps_span_on_stack()
        {
            var traceContext = new TraceContext();
            var span = GetSpan();

            traceContext.Push(span);

            Assert.Equal(span, traceContext.CurrentSpan);
            Assert.Equal(1, traceContext.Count);
        }

        [Fact]
        public void Using_pops_span_from_stack()
        {
            var traceContext = new TraceContext();
            var span = GetSpan();

            using (traceContext.Push(span))
            {
                Assert.Equal(span, traceContext.CurrentSpan);
            }

            Assert.Null(traceContext.CurrentSpan);
        }

        [Fact]
        public async Task Async_Using_pops_span_from_stack()
        {
            var traceContext = new TraceContext();
            var span = GetSpan();

            using (traceContext.Push(span))
            {
                await Delay();
                Assert.Equal(span, traceContext.CurrentSpan);
            }

            await Delay();
            Assert.Null(traceContext.CurrentSpan);
        }

        [Fact]
        public void Sync_CurrentSpan_for_nested_spans_succeeds_with_using()
        {
            var traceContext = new TraceContext();
            var span1 = GetSpan();
            var span2 = GetSpan();

            using (traceContext.Push(span1))
            {
                Assert.Equal(span1, traceContext.CurrentSpan);

                using (traceContext.Push(span2))
                {
                    Assert.Equal(span2, traceContext.CurrentSpan);
                }

                Assert.Equal(span1, traceContext.CurrentSpan);
            }

            Assert.Null(traceContext.CurrentSpan);
        }

        [Fact]
        public async Task Async_CurrentSpan_for_nested_spans_succeeds_with_using()
        {
            var traceContext = new TraceContext();
            var span1 = GetSpan();
            var span2 = GetSpan();
            var span3 = GetSpan();

            using (traceContext.Push(span1))
            {
                await Delay();
                Assert.Equal(span1, traceContext.CurrentSpan);

                await Delay();
                using (traceContext.Push(span2))
                {
                    await Delay();
                    Assert.Equal(span2, traceContext.CurrentSpan);

                    using (traceContext.Push(span3))
                    {
                        await Delay();
                        Assert.Equal(span3, traceContext.CurrentSpan);
                    }

                    await Delay();
                    Assert.Equal(span2, traceContext.CurrentSpan);
                }

                await Delay();
                Assert.Equal(span1, traceContext.CurrentSpan);
            }

            await Delay();
            Assert.Null(traceContext.CurrentSpan);
        }

        public void Sync_CurrentSpan_for_nested_spans_succeeds_with_pop()
        {
            var traceContext = new TraceContext();
            var span1 = GetSpan();
            var span2 = GetSpan();

            traceContext.Push(span1);
            Assert.Equal(span1, traceContext.CurrentSpan);

            traceContext.Push(span2);
            Assert.Equal(span2, traceContext.CurrentSpan);
            traceContext.TryPop();

            Assert.Equal(span1, traceContext.CurrentSpan);
            traceContext.TryPop();

            Assert.Null(traceContext.CurrentSpan);
        }

        [Fact]
        public async Task Async_CurrentSpan_for_nested_spans_succeeds_with_pop()
        {
            var traceContext = new TraceContext();
            var span1 = GetSpan();
            var span2 = GetSpan();
            var span3 = GetSpan();

            traceContext.Push(span1);
            await Delay();

            Assert.Equal(span1, traceContext.CurrentSpan);

            await Delay();
            traceContext.Push(span2);
            await Delay();

            Assert.Equal(span2, traceContext.CurrentSpan);

            traceContext.Push(span3);
            await Delay();

            Assert.Equal(span3, traceContext.CurrentSpan);

            await Delay();
            traceContext.TryPop();

            Assert.Equal(span2, traceContext.CurrentSpan);

            traceContext.TryPop();
            await Delay();

            Assert.Equal(span1, traceContext.CurrentSpan);

            traceContext.TryPop();
            await Delay();

            Assert.Null(traceContext.CurrentSpan);
        }
    }
}