namespace SimpleProjectManager.Server.Data.Data;

public sealed record DbTaskManagerEntry
{
    public string Id { get; init; } = string.Empty;
    public string ManagerId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string JobId { get; init; } = string.Empty;
    public string Info { get; init; } = string.Empty;
}