using Akkatecture.Jobs;
using SimpleProjectManager.Server.Data;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class FilePurgeManager : JobManager<FilePureScheduler, FilePurgeRunner, FilePurgeJob, FilePurgeId>
{
    public FilePurgeManager(IInternalFileRepository bucket, ILogger<FilePurgeManager> logger, IEventAggregator aggregator)
        : base(() => new FilePureScheduler(), () => new FilePurgeRunner(bucket, logger, aggregator)) { }
}