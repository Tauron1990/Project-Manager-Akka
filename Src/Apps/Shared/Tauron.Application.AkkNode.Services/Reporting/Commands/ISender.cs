using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Commands
{
    public interface ISender
    {
        [PublicAPI]
        void SendCommand(IReporterMessage command);
    }
}