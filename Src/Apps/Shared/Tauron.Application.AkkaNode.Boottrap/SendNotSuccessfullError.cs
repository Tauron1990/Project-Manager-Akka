using FluentResults;

namespace Tauron.Application.AkkaNode.Bootstrap;

public sealed class SendNotSuccessfullError : Error
{
    public SendNotSuccessfullError()
    {
        Message = "Sending was not Successfull";
    }
}