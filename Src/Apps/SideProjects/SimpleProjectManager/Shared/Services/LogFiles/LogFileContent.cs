namespace SimpleProjectManager.Shared.Services.LogFiles;

public sealed record LogFileContent(string Content)
{
    public static readonly LogFileContent Empty = new(string.Empty);
}