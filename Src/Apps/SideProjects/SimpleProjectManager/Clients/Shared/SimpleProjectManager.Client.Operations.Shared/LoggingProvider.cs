using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SimpleProjectManager.Client.Operations.Shared;

public class LoggingProvider
{
    public static ILoggerFactory LoggerFactory { get; private set; } = new NullLoggerFactory();
    
    private readonly ILoggerFactory _loggerFactory;

    public LoggingProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public void Run()
        => LoggerFactory = _loggerFactory;
}