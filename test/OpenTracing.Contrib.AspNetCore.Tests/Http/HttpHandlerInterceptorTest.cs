using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.AspNetCore.Interceptors.HttpOut;
using OpenTracing.Mock;
using OpenTracing.Propagation;
using OpenTracing.Tag;
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
            tracer = tracer ?? new MockTracer();

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
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (MockSpan)request.Properties[PropertySpan];
            Assert.NotSame(parentSpan, newSpan);
        }

        [Fact]
        public void OnRequest_span_is_child_of_parent()
        {
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            var parentSpan = tracer.BuildSpan("parent").Start();

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (MockSpan)request.Properties[PropertySpan];

            Assert.NotSame(parentSpan, newSpan);
            Assert.Equal(References.ChildOf, newSpan.References[0].ReferenceType);
            Assert.Same(parentSpan.Context, newSpan.References[0].Context);
        }

        [Fact]
        public void OnRequest_span_has_tags()
        {
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (MockSpan)request.Properties[PropertySpan];

            Assert.Equal("HttpHandler", newSpan.Tags[Tags.Component.Key]);
            Assert.Equal(Tags.SpanKindClient, newSpan.Tags[Tags.SpanKind.Key]);
            Assert.Equal("http://www.example.com/api/values", newSpan.Tags[Tags.HttpUrl.Key]);
            Assert.Equal("GET", newSpan.Tags[Tags.HttpMethod.Key]);
        }

        [Fact]
        public void OnRequest_calls_Inject()
        {
            var propagator = new MockPropagator();
            var tracer = new MockTracer(propagator);
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            Assert.True(propagator.InjectCalled);
        }

        private class MockPropagator : IPropagator
        {
            public bool InjectCalled { get; private set; }

            public bool ExtractCalled { get; private set; }

            public void Inject<TCarrier>(MockSpanContext context, IFormat<TCarrier> format, TCarrier carrier)
            {
                InjectCalled = true;
            }

            public MockSpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
            {
                ExtractCalled = true;
                return null;
            }
        }
    }
}
