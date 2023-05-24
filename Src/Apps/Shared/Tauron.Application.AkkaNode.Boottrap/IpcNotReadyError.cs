using FluentResults;

namespace Tauron.Application.AkkaNode.Bootstrap;

public sealed class IpcNotReadyError : Error
{
    public IpcNotReadyError()
    {
        Message = "Ipc is not Ready";
    }
}