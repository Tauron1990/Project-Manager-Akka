using System;
using Microsoft.Extensions.Logging;

namespace Tauron.Application.AkkaNode.Services.Reporting;

internal sealed partial class ReportingActorLog
{
    private readonly ILogger _logger;

    internal ReportingActorLog(ILogger logger)
        => _logger = logger;

    [LoggerMessage(EventId = 22, Level = LogLevel.Information, Message = "Enter Operation {name} -- {info}")]
    public partial void EnterOperation(string name, string info);

    [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "Process Operation {name} Failed {info}")]
    public partial void FailedOperation(Exception ex, string name, string info);

    [LoggerMessage(EventId = 24, Level = LogLevel.Information, Message = "Exit Operation {name} -- {info}")]
    public partial void ExitOperation(string name, string info);
}