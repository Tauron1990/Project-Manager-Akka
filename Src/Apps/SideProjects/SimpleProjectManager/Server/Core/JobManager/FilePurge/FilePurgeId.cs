using Akkatecture.Jobs;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class FilePurgeId : JobId<FilePurgeId>
{
    private static readonly Guid Namespace = new("E7B269D1-1453-4854-97BD-6CC07ED27A4B");

    public FilePurgeId(string value)
        : base(value) { }

    public static FilePurgeId For(ProjectFileId id)
        => NewDeterministic(Namespace, id.Value);
}