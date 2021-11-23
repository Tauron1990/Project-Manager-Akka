using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;

namespace SimpleProjectManager.Server.Core.Services;

public class JobFileService : IJobFileService, IDisposable
{
    private readonly ILogger<JobFileService> _logger;
    private readonly IMongoCollection<FileInfoData> _files;
    private readonly IDisposable _subscription;

    public JobFileService(InternalDataRepository dataRepository, ILogger<JobFileService> logger, IEventAggregator aggregator)
    {
        _logger = logger;
        _files = dataRepository.Collection<FileInfoData>();
        _subscription = aggregator.SubscribeTo<FileDeleted>()
           .ObserveOn(Scheduler.Default)
           .Subscribe(
                d =>
                {
                    var filter = Builders<FileInfoData>.Filter.Eq(m => m.Id, d.Id);
                    var result = _files.DeleteOne(filter);

                    if (!result.IsAcknowledged || result.DeletedCount != 1) return;

                    using (Computed.Invalidate())
                        GetJobFileInfo(d.Id, default).Ignore();
                });
    }

    public async ValueTask<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
    {
        if (Computed.IsInvalidating()) return null;
        
        var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, id);
        var result = await _files.Find(filter).FirstOrDefaultAsync(token);

        return result == null 
            ? null 
            : new ProjectFileInfo(result.Id, result.ProjectName, result.FileName, result.Size, result.FileType, result.Mime);
    }

    public async ValueTask<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
    {
        try
        {
            var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, projectFile.Id);

            if (await _files.CountDocumentsAsync(filter, cancellationToken: token) == 1)
                return "Der eintrag existiert schon";

            await _files.InsertOneAsync(
                new FileInfoData
                {
                    Id = projectFile.Id,
                    ProjectName = projectFile.ProjectName,
                    FileName = projectFile.FileName,
                    Size = projectFile.Size,
                    FileType = projectFile.FileType,
                    Mime = projectFile.Mime
                },
                cancellationToken: token);

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on Registrationg File");
            return ex.Message;
        }
    }

    public void Dispose()
        => _subscription.Dispose();
}