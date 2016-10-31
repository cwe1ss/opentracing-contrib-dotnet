using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using OpenTracing.Instrumentation.Http;
using OpenTracing.Testing;
using Xunit;

namespace OpenTracing.Instrumentation.Tests.Http
{
    public class HttpHandlerInterceptorTest
    {
        private const string PropertySpan = "ot-span";

        private HttpHandlerInterceptor GetInterceptor(
            ITracer tracer = null,
            ITraceContext traceContext = null
        )
        {
            var loggerFactory = new LoggerFactory();
            tracer = tracer ?? new TestTracer();
            traceContext = traceContext ?? new TraceContext();

            return new HttpHandlerInterceptor(loggerFactory, tracer, traceContext);
        }

        [Fact]
        public void OnRequest_creates_span_if_no_parent()
        {
            var interceptor = GetInterceptor();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.NotNull(request.Properties[PropertySpan]);
        }

        [Fact]
        public void OnRequest_creates_span_if_parent()
        {
            var tracer = new TestTracer();
            var traceContext = new TraceContext();
            var interceptor = GetInterceptor(tracer, traceContext);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            traceContext.Push(parentSpan);

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (TestSpan)request.Properties[PropertySpan];
            Assert.NotSame(parentSpan, newSpan);
        }

        [Fact]
        public void OnRequest_span_is_child_of_parent()
        {
            var tracer = new TestTracer();
            var traceContext = new TraceContext();
            var interceptor = GetInterceptor(tracer, traceContext);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();
            traceContext.Push(parentSpan);

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (TestSpan)request.Properties[PropertySpan];

            Assert.NotSame(parentSpan, newSpan);
            Assert.Equal(References.ChildOf, newSpan.TypedContext.References[0].ReferenceType);
            Assert.Same(parentSpan.Context, newSpan.TypedContext.References[0].ReferencedContext);
        }

        [Fact]
        public void OnRequest_span_has_tags()
        {
            var tracer = new TestTracer();
            var traceContext = new TraceContext();
            var interceptor = GetInterceptor(tracer, traceContext);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (TestSpan)request.Properties[PropertySpan];

            Assert.Equal("HttpHandler", newSpan.GetStringTag(Tags.Component));
            Assert.Equal(Tags.SpanKindClient, newSpan.GetStringTag(Tags.SpanKind));
            Assert.Equal("http://www.example.com/api/values", newSpan.GetStringTag(Tags.HttpUrl));
            Assert.Equal("GET", newSpan.GetStringTag(Tags.HttpMethod));
        }

        [Fact]
        public void OnRequest_calls_Inject()
        {
            var tracer = new TestTracer();
            var traceContext = new TraceContext();
            var interceptor = GetInterceptor(tracer, traceContext);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            Assert.True(tracer.InjectCalled);
        }
    }
}