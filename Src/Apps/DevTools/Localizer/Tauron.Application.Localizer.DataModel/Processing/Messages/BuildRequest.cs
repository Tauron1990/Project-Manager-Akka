using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record BuildRequest(IObservable<string> OperationId, ProjectFile ProjectFile);
}