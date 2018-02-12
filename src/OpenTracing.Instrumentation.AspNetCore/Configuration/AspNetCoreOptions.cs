namespace OpenTracing.Instrumentation.AspNetCore.Configuration
{
    public class AspNetCoreOptions
    {
        public bool StartAutomatically { get; set; }

        public AspNetCoreOptions()
        {
            // Defaults
            StartAutomatically = true;
        }
    }
}
