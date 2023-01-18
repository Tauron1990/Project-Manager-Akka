using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SimpleProjectManager.Operation.Client;

internal class LoggingProvider
{
    internal static ILoggerFactory LoggerFactory { get; private set; } = new NullLoggerFactory();
    
    private readonly ILoggerFactory _loggerFactory;

    internal LoggingProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    internal void Run()
        => LoggerFactory = _loggerFactory;
}