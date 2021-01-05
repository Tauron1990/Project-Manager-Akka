using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record LoadedProjectFile(string OperationId, ProjectFile ProjectFile, Exception? ErrorReason, bool Ok) : Operation(OperationId);
}