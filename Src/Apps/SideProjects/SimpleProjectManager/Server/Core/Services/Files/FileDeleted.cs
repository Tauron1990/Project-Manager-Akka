using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record FileDeleted(ProjectFileId Id);

public sealed record FileAdded
{
    public static FileAdded Inst = new();
}