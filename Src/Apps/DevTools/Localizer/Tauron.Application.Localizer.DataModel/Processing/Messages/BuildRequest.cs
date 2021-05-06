using System;

namespace Tauron.Application.Localizer.DataModel.Processing.Messages
{
    public sealed record BuildRequest(IObservable<string> OperationId, ProjectFile ProjectFile);
}