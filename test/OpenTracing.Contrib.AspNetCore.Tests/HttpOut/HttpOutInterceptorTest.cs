using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTracing.Contrib.Core.Configuration;
using OpenTracing.Contrib.Core.Interceptors.HttpOut;
using OpenTracing.Mock;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using OpenTracing.Util;
using Xunit;

namespace OpenTracing.Contrib.AspNetCore.Tests.HttpOut
{
    public class HttpOutInterceptorTest
    {
        private HttpOutInterceptor GetInterceptor(
            ITracer tracer = null,
            HttpOutOptions options = null)
        {
            var loggerFactory = new LoggerFactory();
            tracer = tracer ?? new MockTracer();
            options = options ?? new HttpOutOptions();

            return new HttpOutInterceptor(loggerFactory, tracer, Options.Create(options));
        }

        [Fact]
        public void OnRequest_creates_span_if_no_parent()
        {
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.NotNull(tracer.ActiveSpan);
        }

        [Fact]
        public void OnRequest_span_is_child_of_parent()
        {
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Create parent span
            using (var scope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                var parentSpan = scope.Span;

                // Call interceptor
                interceptor.OnRequest(request);

                var newSpan = (MockSpan)tracer.ActiveSpan;

                Assert.NotSame(parentSpan, newSpan);
                Assert.Single(newSpan.References);
                Assert.Equal(References.ChildOf, newSpan.References[0].ReferenceType);
                Assert.Same(parentSpan.Context, newSpan.References[0].Context);
            }
        }

        [Fact]
        public void OnRequest_span_has_tags()
        {
            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            var newSpan = (MockSpan)tracer.ActiveSpan;

            Assert.Equal("HttpHandler", newSpan.Tags[Tags.Component.Key]);
            Assert.Equal(Tags.SpanKindClient, newSpan.Tags[Tags.SpanKind.Key]);
            Assert.Equal("http://www.example.com/api/values", newSpan.Tags[Tags.HttpUrl.Key]);
            Assert.Equal("GET", newSpan.Tags[Tags.HttpMethod.Key]);
        }

        [Fact]
        public void OnRequest_calls_Inject()
        {
            var propagator = Substitute.For<IPropagator>();
            var tracer = new MockTracer(new AsyncLocalScopeManager(), propagator);
            var interceptor = GetInterceptor(tracer);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            // Call interceptor
            interceptor.OnRequest(request);

            propagator.Received(1).Inject(Arg.Any<MockSpanContext>(), BuiltinFormats.HttpHeaders, Arg.Any<HttpHeadersCarrier>());
        }

        [Fact]
        public void OnRequest_calls_Options_ShouldIgnore()
        {
            bool shouldIgnoreCalled = false;

            var options = new HttpOutOptions();
            options.ShouldIgnore.Clear();
            options.ShouldIgnore.Add(_ => shouldIgnoreCalled = true);

            var interceptor = GetInterceptor(options: options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.True(shouldIgnoreCalled);
        }

        [Fact]
        public void OnRequest_calls_Options_ShouldIgnore_list_correctly()
        {
            bool shouldIgnore1Called = false;
            bool shouldIgnore2Called = false;
            bool shouldIgnore3Called = false;

            var options = new HttpOutOptions();
            options.ShouldIgnore.Clear();
            options.ShouldIgnore.Add(_ => { shouldIgnore1Called = true; return false; });
            options.ShouldIgnore.Add(_ => { shouldIgnore2Called = true; return true; });
            options.ShouldIgnore.Add(_ => { shouldIgnore3Called = true; return false; });

            var interceptor = GetInterceptor(options: options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.True(shouldIgnore1Called);
            Assert.True(shouldIgnore2Called);
            Assert.False(shouldIgnore3Called);
        }

        [Fact]
        public void OnRequest_calls_Options_OperationNameResolver()
        {
            bool operationNameResolverCalled = false;

            var options = new HttpOutOptions();
            options.OperationNameResolver = _ =>
            {
                operationNameResolverCalled = true;
                return "foo";
            };

            var interceptor = GetInterceptor(options: options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.True(operationNameResolverCalled);
        }

        [Fact]
        public void OnRequest_calls_Options_OnRequest()
        {
            bool onRequestCalled = false;

            var options = new HttpOutOptions();
            options.OnRequest = (_, __) => onRequestCalled = true;

            var interceptor = GetInterceptor(options: options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.True(onRequestCalled);
        }

        [Fact]
        public void OnRequest_does_not_create_span_if_Options_ShouldIgnore_returns_true()
        {
            var options = new HttpOutOptions();
            options.ShouldIgnore.Clear();
            options.ShouldIgnore.Add(_ => true);

            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer, options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            Assert.Null(tracer.ActiveSpan);
        }

        [Fact]
        public void OnRequest_sets_OperationName_from_Options_OperationNameResolver()
        {
            var options = new HttpOutOptions();
            options.OperationNameResolver = _ => "foo";

            var tracer = new MockTracer();
            var interceptor = GetInterceptor(tracer, options);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://www.example.com/api/values"));

            interceptor.OnRequest(request);

            var newSpan = (MockSpan)tracer.ActiveSpan;

            Assert.Equal("foo", newSpan.OperationName);
        }
    }
}
