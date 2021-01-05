namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record SaveProject(string OperationId, ProjectFile projectFile) : Operation(OperationId);
}