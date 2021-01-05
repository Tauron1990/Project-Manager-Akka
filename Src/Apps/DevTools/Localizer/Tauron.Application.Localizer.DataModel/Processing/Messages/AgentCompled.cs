using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record AgentCompled(bool Failed, Exception? Cause, string OperationId);
}