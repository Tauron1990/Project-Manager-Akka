using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Akkatecture.Jobs;
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
    private readonly FileContentManager _contentManager;
    private readonly IMongoCollection<FileInfoData> _files;
    private readonly IDisposable _subscription;

    public JobFileService(InternalDataRepository dataRepository, ILogger<JobFileService> logger, IEventAggregator aggregator, FileContentManager contentManager)
    {
        _logger = logger;
        _contentManager = contentManager;
        _files = dataRepository.Collection<FileInfoData>();
        _subscription = new CompositeDisposable
                        {
                            aggregator.SubscribeTo<FileAdded>()
                               .ObserveOn(Scheduler.Default)
                               .Subscribe(
                                    _ =>
                                    {
                                        using (Computed.Invalidate())
                                            GetAllFiles(default).Ignore();
                                    }),
                            
                            aggregator.SubscribeTo<FileDeleted>()
                               .ObserveOn(Scheduler.Default)
                               .Subscribe(
                                    d =>
                                    {
                                        using (Computed.Invalidate())
                                            GetAllFiles(default).Ignore();

                                        var filter = Builders<FileInfoData>.Filter.Eq(m => m.Id, d.Id);
                                        var result = _files.DeleteOne(filter);

                                        if (!result.IsAcknowledged || result.DeletedCount != 1) return;

                                        using (Computed.Invalidate())
                                            GetJobFileInfo(d.Id, default).Ignore();
                                    })
                        };
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

    public virtual async ValueTask<DatabaseFile[]> GetAllFiles(CancellationToken token)
    {
        var files = new List<DatabaseFile>();

        await (await _contentManager.QueryFiles(token)).ForEachAsync(
            d => files.Add(
                new DatabaseFile(
                    new ProjectFileId(d.Filename),
                    new FileName(d.Metadata.GetValue(FileContentManager.MetaFileNme).AsString),
                    new FileSize(d.Length),
                    new JobName(d.Metadata.GetValue(FileContentManager.MetaJobName).AsString))),
            token);

        return files.ToArray();
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

    private static async ValueTask<string> AggregateErrors<TItem>(IEnumerable<TItem> items, Func<TItem, ValueTask<string?>> executor)
    {
        var errors = ImmutableList<string>.Empty;

        foreach (var item in items)
        {
            var result = await executor(item);
            if(string.IsNullOrWhiteSpace(result)) continue;

            errors = errors.Add(result);
        }
        
        return errors.IsEmpty ? string.Empty : string.Join($", {Environment.NewLine}", errors);
    }

    public async ValueTask<string> CommitFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.CommitFile(id, token));

    public async ValueTask<string> DeleteFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.DeleteFile(id, token));

    public void Dispose()
        => _subscription.Dispose();
}