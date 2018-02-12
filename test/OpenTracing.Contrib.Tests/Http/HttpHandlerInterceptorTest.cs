using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.Http;
using OpenTracing.Tag;
using OpenTracing.Testing;
using Xunit;

namespace OpenTracing.Contrib.Tests.Http
{
    public class HttpHandlerInterceptorTest
    {
        private const string PropertySpan = "ot-span";

        private HttpHandlerInterceptor GetInterceptor(
            ITracer tracer = null)
        {
            var loggerFactory = new LoggerFactory();
            tracer = tracer ?? new TestTracer();

            return new HttpHandlerInterceptor(loggerFactory, tracer);
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
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (TestSpan)request.Properties[PropertySpan];
            Assert.NotSame(parentSpan, newSpan);
        }

        [Fact]
        public void OnRequest_span_is_child_of_parent()
        {
            var tracer = new TestTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();

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
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (TestSpan)request.Properties[PropertySpan];

            Assert.Equal("HttpHandler", newSpan.GetStringTag(Tags.Component.Key));
            Assert.Equal(Tags.SpanKindClient, newSpan.GetStringTag(Tags.SpanKind.Key));
            Assert.Equal("http://www.example.com/api/values", newSpan.GetStringTag(Tags.HttpUrl.Key));
            Assert.Equal("GET", newSpan.GetStringTag(Tags.HttpMethod.Key));
        }

        [Fact]
        public void OnRequest_calls_Inject()
        {
            var tracer = new TestTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            Assert.True(tracer.InjectCalled);
        }
    }
}
