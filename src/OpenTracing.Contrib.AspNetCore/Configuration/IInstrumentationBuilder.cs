namespace Microsoft.Extensions.DependencyInjection
{
    public interface IInstrumentationBuilder
    {
        IServiceCollection Services { get; }
    }
}
