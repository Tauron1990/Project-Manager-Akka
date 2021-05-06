namespace Tauron.Application.Localizer.DataModel.Processing.Messages
{
    public sealed record SaveProject(string OperationId, ProjectFile ProjectFile) : Operation(OperationId);
}