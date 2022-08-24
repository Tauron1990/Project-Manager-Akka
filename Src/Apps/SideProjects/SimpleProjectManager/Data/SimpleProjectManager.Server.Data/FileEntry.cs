namespace SimpleProjectManager.Server.Data;

public sealed record FileEntry(string Id, string FileId, string JobName, string FileName, long Length);