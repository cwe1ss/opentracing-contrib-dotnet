using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenTracing.Contrib.Http;
using OpenTracing.Contrib.Testing;
using Xunit;

namespace OpenTracing.Contrib.Tests.Http
{
    public class OpenTracingDelegatingHandlerTest
    {
        /// <summary>
        /// SendAsync in base class is protected, so we need to make it public here.
        /// </summary>
        private class TestOpenTracingDelegatingHandler : OpenTracingDelegatingHandler
        {
            public TestOpenTracingDelegatingHandler(
                ITracer tracer,
                ISpanContextAccessor spanContextAccessor,
                Func<HttpRequestMessage, HttpResponseMessage> callback = null)
                : base(tracer, spanContextAccessor)
            {
                InnerHandler = new CallbackMessageHandler(callback);
            }

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                return SendAsync(request, CancellationToken.None);
            }

            private class CallbackMessageHandler : HttpMessageHandler
            {
                private readonly Func<HttpRequestMessage, HttpResponseMessage> _callback;

                public CallbackMessageHandler(
                    Func<HttpRequestMessage, HttpResponseMessage> callback = null)
                {
                    _callback = callback ?? new Func<HttpRequestMessage, HttpResponseMessage>(_ =>
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    });
                }

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    HttpResponseMessage response = _callback(request);
                    return Task.FromResult(response);
                }
            }
        }

        [Fact]
        public void Ctor_throws_if_parameters_missing()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();

            Assert.Throws<ArgumentNullException>(() => new OpenTracingDelegatingHandler(null, contextAccessor));
            Assert.Throws<ArgumentNullException>(() => new OpenTracingDelegatingHandler(tracer, null));
        }

        [Fact]
        public async Task SendAsync_does_not_create_span_if_no_parent()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            var response = await handler.SendAsync(request);

            Assert.Empty(tracer.FinishedSpans);
        }

        [Fact]
        public async Task SendAsync_creates_span_if_parent()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API
            var response = await handler.SendAsync(request);

            Assert.Equal(1, tracer.FinishedSpans.Count);
        }

        [Fact]
        public async Task Success_span_is_child_of_parent()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API
            var response = await handler.SendAsync(request);

            var newSpan = tracer.FinishedSpans[0];

            Assert.NotSame(parentSpan, newSpan);
            Assert.Equal(References.ChildOf, newSpan.References[0].Key);
            Assert.Same(parentSpan.Context, newSpan.References[0].Value);
        }

        [Fact]
        public async Task Success_span_has_tags()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API
            var response = await handler.SendAsync(request);

            Assert.Equal(1, tracer.FinishedSpans.Count);

            var finishedSpan = tracer.FinishedSpans[0];
            Assert.Equal("HttpClient", finishedSpan.Tags[Tags.Component]);
            Assert.Equal(Tags.SpanKindClient, finishedSpan.Tags[Tags.SpanKind]);
            Assert.Equal("http://www.example.com/api/values", finishedSpan.Tags[Tags.HttpUrl]);
            Assert.Equal("GET", finishedSpan.Tags[Tags.HttpMethod]);
        }

        [Fact]
        public async Task SendAsync_creates_span_on_exception()
        {
            var callback = new Func<HttpRequestMessage, HttpResponseMessage>(_ =>
            {
                throw new InvalidOperationException("invalid operation");
            });

            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor, callback);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API and catch exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.SendAsync(request));

            Assert.Equal(1, tracer.FinishedSpans.Count);
        }

        [Fact]
        public async Task Error_span_has_tags()
        {
            var callback = new Func<HttpRequestMessage, HttpResponseMessage>(_ =>
            {
                throw new InvalidOperationException();
            });

            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor, callback);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API and catch exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.SendAsync(request));

            Assert.Equal(1, tracer.FinishedSpans.Count);

            var finishedSpan = tracer.FinishedSpans[0];
            Assert.Equal("HttpClient", finishedSpan.Tags[Tags.Component]); // regular tags should be set before invocation.
            Assert.Equal(true, finishedSpan.Tags[Tags.Error]);
        }

        [Fact]
        public async Task SendAsync_calls_Inject()
        {
            var tracer = new TestTracer();
            var contextAccessor = new SpanContextAccessor();
            var handler = new TestOpenTracingDelegatingHandler(tracer, contextAccessor);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            contextAccessor.SpanContext = parentSpan.Context;

            // Call API
            var response = await handler.SendAsync(request);

            Assert.True(tracer.InjectCalled);
        }
    }
}