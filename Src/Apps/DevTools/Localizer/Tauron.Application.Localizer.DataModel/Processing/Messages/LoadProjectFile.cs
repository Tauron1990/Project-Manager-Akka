using JetBrains.Annotations;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    [PublicAPI]
    public sealed record LoadProjectFile(string OperationId, string Source) : Operation(OperationId);
}