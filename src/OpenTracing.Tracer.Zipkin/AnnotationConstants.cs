// From https://github.com/openzipkin/zipkin-csharp/blob/master/src/Zipkin.Tracer/AnnotationConstants.cs

namespace OpenTracing.Tracer.Zipkin
{
    public static class AnnotationConstants
    {
        /// <summary>
        /// The client sent ("cs") a request to a server. There is only one send per
        /// span. For example, if there's a transport error, each attempt can be logged
        /// as a <see cref="WireSend"/> annotation.
        ///
        /// If chunking is involved, each chunk could be logged as a separate
        /// <see cref="ClientSendFragment"/> in the same span.
        ///
        /// Annotation.host is not the server. It is the host which logged the send
        /// event, almost always the client. When logging <see cref="ClientSend"/>, instrumentation
        /// should also log the <see cref="ServerAddress"/>.
        /// </summary>
        public const string ClientSend = "cs";

        /// <summary>
        /// The client received ("cr") a response from a server. There is only one
        /// receive per span. For example, if duplicate responses were received, each
        /// can be logged as a <see cref="WireReceive"/> annotation.
        ///
        /// If chunking is involved, each chunk could be logged as a separate
        /// <see cref="ClientReceiveFragment"/> in the same span.
        ///
        /// Annotation.host is not the server. It is the host which logged the receive
        /// event, almost always the client. The actual endpoint of the server is
        /// recorded separately as <see cref="ServerAddress"/> when <see cref="ClientSend"/> is logged.
        /// </summary>
        public const string ClientReceive = "cr";

        /// <summary>
        /// The server sent ("ss") a response to a client. There is only one response
        /// per span. If there's a transport error, each attempt can be logged as a
        /// <see cref="WireSend"/> annotation.
        ///
        /// Typically, a trace ends with a server send, so the last timestamp of a trace
        /// is often the timestamp of the root span's server send.
        ///
        /// If chunking is involved, each chunk could be logged as a separate
        /// <see cref="ServerSendFragment"/> in the same span.
        ///
        /// Annotation.host is not the client. It is the host which logged the send
        /// event, almost always the server. The actual endpoint of the client is
        /// recorded separately as <see cref="ClientAddress"/> when <see cref="ServerReceive"/> is logged.
        /// </summary>
        public const string ServerSend = "ss";

        /// <summary>
        /// The server received ("sr") a request from a client. There is only one
        /// request per span.  For example, if duplicate responses were received, each
        /// can be logged as a <see cref="WireReceive"/> annotation.
        ///
        /// Typically, a trace starts with a server receive, so the first timestamp of a
        /// trace is often the timestamp of the root span's server receive.
        ///
        /// If chunking is involved, each chunk could be logged as a separate
        /// <see cref="ServerReceiveFragment"/> in the same span.
        ///
        /// Annotation.host is not the client. It is the host which logged the receive
        /// event, almost always the server. When logging <see cref="ServerReceive"/>, instrumentation
        /// should also log the <see cref="ClientAddress"/>.
        /// </summary>
        public const string ServerReceive = "sr";

        /// <summary>
        /// Optionally logs an attempt to send a message on the wire. Multiple wire send
        /// events could indicate network retries. A lag between client or server send
        /// and wire send might indicate queuing or processing delay.
        /// </summary>
        public const string WireSend = "ws";

        /// <summary>
        /// Optionally logs an attempt to receive a message from the wire. Multiple wire
        /// receive events could indicate network retries. A lag between wire receive
        /// and client or server receive might indicate queuing or processing delay.
        /// </summary>
        public const string WireReceive = "wr";

        /// <summary>
        /// Optionally logs progress of a (<see cref="ClientSend"/>, <see cref="WireSend"/>). For example, this
        /// could be one chunk in a chunked request.
        /// </summary>
        public const string ClientSendFragment = "csf";

        /// <summary>
        /// Optionally logs progress of a (<see cref="ClientReceive"/>, <see cref="WireReceive"/>). For example, this
        /// could be one chunk in a chunked response.
        /// </summary>
        public const string ClientReceiveFragment = "crf";

        /// <summary>
        /// Optionally logs progress of a (<see cref="ServerSend"/>, <see cref="WireSend"/>). For example, this
        /// could be one chunk in a chunked response.
        /// </summary>
        public const string ServerSendFragment = "ssf";

        /// <summary>
        /// Optionally logs progress of a (<see cref="ServerReceive"/>, <see cref="WireReceive"/>). For example, this
        /// could be one chunk in a chunked request.
        /// </summary>
        public const string ServerReceiveFragment = "srf";

        /// <summary>
        /// The domain portion of the URL or host header. Ex. "mybucket.s3.amazonaws.com"
        ///
        /// Used to filter by host as opposed to ip address.
        /// </summary>
        public const string HttpHost = "http.host";

        /// <summary>
        /// The HTTP method, or verb, such as "GET" or "POST".
        ///
        /// Used to filter against an http route.
        /// </summary>
        public const string HttpMethod = "http.method";

        /// <summary>
        /// The absolute http path, without any query parameters. Ex. "/objects/abcd-ff"
        ///
        /// Used to filter against an http route, portably with zipkin v1.
        ///
        /// In zipkin v1, only equals filters are supported. Dropping query parameters makes the number
        /// of distinct URIs less. For example, one can query for the same resource, regardless of signing
        /// parameters encoded in the query line. This does not reduce cardinality to a HTTP single route.
        /// For example, it is common to express a route as an http URI template like
        /// "/resource/{resource_id}". In systems where only equals queries are available, searching for
        /// http/path=/resource won't match if the actual request was /resource/abcd-ff.
        ///
        /// Historical note: This was commonly expressed as "http.uri" in zipkin, eventhough it was most
        /// often just a path.
        /// </summary>
        public const string HttpPath = "http.path";

        /// <summary>
        /// The entire URL, including the scheme, host and query parameters if available. Ex.
        /// "https://mybucket.s3.amazonaws.com/objects/abcd-ff?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Algorithm=AWS4-HMAC-SHA256..."
        ///
        /// Combined with HTTP_METHOD, you can understand the fully-qualified request line.
        ///
        /// This is optional as it may include private data or be of considerable length.
        /// </summary>
        public const string HttpUrl = "http.url";

        /// <summary>
        /// The HTTP status code, when not in 2xx range. Ex. "503"
        ///
        /// Used to filter for error status.
        /// </summary>
        public const string HttpStatusCode = "http.status_code";

        /// <summary>
        /// The size of the non-empty HTTP request body, in bytes. Ex. "16384"
        ///
        /// Large uploads can exceed limits or contribute directly to latency.
        /// </summary>
        public const string HttpRequestSize = "http.request.size";

        /// <summary>
        /// The size of the non-empty HTTP response body, in bytes. Ex. "16384"
        ///
        /// Large downloads can exceed limits or contribute directly to latency.
        /// </summary>
        public const string HttpResponseSize = "http.response.size";

        /// <summary>
        /// The value of "lc" is the component or namespace of a local span.
        ///
        /// BinaryAnnotation.host adds service context needed to support queries.
        ///
        /// Local Component("lc") supports three key features: flagging, query by
        /// service and filtering Span.name by namespace.
        ///
        /// While structurally the same, local spans are fundamentally different than
        /// RPC spans in how they should be interpreted. For example, zipkin v1 tools
        /// center on RPC latency and service graphs. Root local-spans are neither
        /// indicative of critical path RPC latency, nor have impact on the shape of a
        /// service graph. By flagging with "lc", tools can special-case local spans.
        ///
        /// Zipkin v1 Spans are unqueryable unless they can be indexed by service name.
        /// The only path to a service name is by (Binary)?Annotation.host.serviceName.
        /// By logging "lc", a local span can be queried even if no other annotations
        /// are logged.
        ///
        /// The value of "lc" is the namespace of Span.name. For example, it might be
        /// "finatra2", for a span named "bootstrap". "lc" allows you to resolves
        /// conflicts for the same Span.name, for example "finatra/bootstrap" vs
        /// "finch/bootstrap". Using local component, you'd search for spans named
        /// "bootstrap" where "lc=finch"
        /// </summary>
        public const string LocalComponent = "lc";

        /// <summary>
        /// Indicates a client address ("ca") in a span. Most likely, there's only one.
        /// Multiple addresses are possible when a client changes its ip or port within
        /// a span.
        /// </summary>
        public const string ClientAddress = "ca";

        /// <summary>
        /// Indicates a server address ("sa") in a span. Most likely, there's only one.
        /// Multiple addresses are possible when a client is redirected, or fails to a
        /// different server ip or port.
        /// </summary>
        public const string ServerAddress = "sa";

    }
}
