using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Commands
{
    public interface ISender
    {
        [PublicAPI]
        void SendCommand(IReporterMessage command);
    }
}