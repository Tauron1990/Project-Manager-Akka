using System;

namespace Tauron.Application.Localizer.DataModel.Processing.Messages
{
    public sealed record AgentCompled(bool Failed, Exception? Cause, string OperationId);
}