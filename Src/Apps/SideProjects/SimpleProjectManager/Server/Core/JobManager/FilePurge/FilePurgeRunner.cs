using Akkatecture.Jobs;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Data;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class FilePurgeRunner : JobRunner<FilePurgeJob, FilePurgeId>, IRun<FilePurgeJob>
{
    private readonly IEventAggregator _aggregator;
    private readonly IInternalFileRepository _bucket;
    private readonly ILogger _logger;

    public FilePurgeRunner(IInternalFileRepository bucket, ILogger logger, IEventAggregator aggregator)
    {
        _bucket = bucket;
        _logger = logger;
        _aggregator = aggregator;
    }

    public bool Run(FilePurgeJob job)
    {
        string? search = _bucket.FindIdByFileName(job.FileToDelete.Value).FirstOrDefault();
        if(string.IsNullOrEmpty(search))
        {
            _logger.LogWarning("File with Name {Id} not found", job.FileToDelete.Value);

            return false;
        }

        _bucket.Delete(search);
        _aggregator.Publish(new FileDeleted(job.FileToDelete));

        return true;
    }
}