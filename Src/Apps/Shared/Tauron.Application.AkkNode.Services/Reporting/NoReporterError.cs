using FluentResults;

namespace Tauron.Application.AkkaNode.Services.Reporting;

public sealed class NoReporterError : Error
{
    public NoReporterError()
    {
        Message = "No Reporter";
    }
}