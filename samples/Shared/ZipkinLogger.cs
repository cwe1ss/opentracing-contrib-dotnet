using Microsoft.Extensions.Logging;

namespace Shared
{
    /// <summary>
    /// Forwards log messages from "zipkin4net" to "Microsoft.Extensions.Logging".
    /// </summary>
    public class ZipkinLogger : zipkin4net.ILogger
    {
        private readonly ILogger _logger;

        public ZipkinLogger(ILoggerFactory loggerFactory, string loggerName)
        {
            _logger = loggerFactory.CreateLogger(loggerName);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
