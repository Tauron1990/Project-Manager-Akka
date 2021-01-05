using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record SavedProject(string OperationId, bool Ok, Exception? Exception) : Operation(OperationId);
}