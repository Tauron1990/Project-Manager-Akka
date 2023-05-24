using FluentResults;

namespace Tauron.Application.AkkaNode.Bootstrap;

public sealed class NoServerOrClientError : Error
{
    public NoServerOrClientError()
    {
        Message = "No Server or Client Configured";
    }
}