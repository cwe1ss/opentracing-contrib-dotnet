namespace OpenTracing.Tracer.Zipkin.Configuration
{
    public interface IEndpointResolver
    {
        Endpoint GetEndpoint();
    }
}