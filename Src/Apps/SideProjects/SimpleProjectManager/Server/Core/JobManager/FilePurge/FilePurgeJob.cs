using Akkatecture.Jobs;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Data;
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

public sealed record FilePurgeJob(ProjectFileId FileToDelete) : IJob;

public sealed class FilePurgeRunner : JobRunner<FilePurgeJob, FilePurgeId>, IRun<FilePurgeJob>
{
    private readonly IInternalFileRepository _bucket;
    private readonly ILogger _logger;
    private readonly IEventAggregator _aggregator;

    public FilePurgeRunner(IInternalFileRepository bucket, ILogger logger, IEventAggregator aggregator)
    {
        _bucket = bucket;
        _logger = logger;
        _aggregator = aggregator;
    }

    public bool Run(FilePurgeJob job)
    {
        var search = _bucket.FindIdByFileName(job.FileToDelete.Value).FirstOrDefault();
        if (string.IsNullOrEmpty(search))
        {
            _logger.LogWarning("File with Name {Id} not found", job.FileToDelete.Value);
            return false;
        }

        _bucket.Delete(search);
        _aggregator.Publish(new FileDeleted(job.FileToDelete));
        return true;
    }
}

public sealed class FilePureScheduler : JobScheduler<FilePureScheduler, FilePurgeJob, FilePurgeId>
{}

public sealed class FilePurgeManager : JobManager<FilePureScheduler, FilePurgeRunner, FilePurgeJob, FilePurgeId>
{
    public FilePurgeManager(IInternalFileRepository bucket, ILogger<FilePurgeManager> logger, IEventAggregator aggregator)
        : base(() => new FilePureScheduler(), () => new FilePurgeRunner(bucket, logger, aggregator))
    {
        
    }
}