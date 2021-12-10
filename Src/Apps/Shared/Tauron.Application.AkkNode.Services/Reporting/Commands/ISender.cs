using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting.Commands;

public interface ISender
{
    [PublicAPI]
    void SendCommand(IReporterMessage command);
}