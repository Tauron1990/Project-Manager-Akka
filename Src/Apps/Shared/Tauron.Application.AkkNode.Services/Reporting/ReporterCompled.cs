using FluentResults;

namespace Tauron.Application.AkkaNode.Services.Reporting;

public sealed class ReporterCompledError : Error
{
    public ReporterCompledError()
    {
        Message = "Reporter Compledted";
    }
}