using System;

namespace Tauron.Application.Localizer.DataModel.Processing.Messages
{
    public sealed record SavedProject(string OperationId, bool Ok, Exception? Exception) : Operation(OperationId);
}