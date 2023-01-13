namespace SimpleProjectManager.Shared.Services.Tasks;

public sealed record TaskManagerEntry
{
    public string Id { get; init; } = string.Empty;
    public string ManagerId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string JobId { get; init; } = string.Empty;
    public string Info { get; init; } = string.Empty;
}