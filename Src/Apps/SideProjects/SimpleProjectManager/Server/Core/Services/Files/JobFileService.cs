using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Akkatecture.Jobs;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Services;

public class JobFileService : IJobFileService, IDisposable
{
    private readonly FileContentManager _contentManager;
    private readonly MappingDatabase<DbFileInfoData, FileInfoData> _files;
    private readonly ILogger<JobFileService> _logger;
    private readonly IDisposable _subscription;

    public JobFileService(IInternalDataRepository dataRepository, ILogger<JobFileService> logger, IEventAggregator aggregator, FileContentManager contentManager)
    {
        _logger = logger;
        _contentManager = contentManager;
        _files = new MappingDatabase<DbFileInfoData, FileInfoData>(
            dataRepository.Databases.FileInfos,
            dataRepository.Databases.Mapper);

        _subscription = new CompositeDisposable
                        {
                            aggregator.SubscribeTo<FileAdded>()
                               .ObserveOn(Scheduler.Default)
                               .Subscribe(
                                    _ =>
                                    {
                                        using (Computed.Invalidate())
                                        {
                                            GetAllFiles(default).Ignore();
                                        }
                                    }),

                            aggregator.SubscribeTo<FileDeleted>()
                               .ObserveOn(Scheduler.Default)
                               .Subscribe(
                                    d =>
                                    {
                                        using (Computed.Invalidate())
                                        {
                                            GetAllFiles(default).Ignore();
                                        }

                                        var filter = _files.Operations.Eq(m => m.Id, d.Id.Value);
                                        DbOperationResult result = _files.DeleteOne(filter);

                                        if(!result.IsAcknowledged || result.DeletedCount != 1) return;

                                        using (Computed.Invalidate())
                                        {
                                            GetJobFileInfo(d.Id, default).Ignore();
                                        }
                                    })
                        };
    }

    public void Dispose()
        => _subscription.Dispose();

    public virtual async Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
    {
        if(Computed.IsInvalidating()) return null;

        var filter = _files.Operations.Eq(d => d.Id, id.Value);

        return await _files.ExecuteFirstOrDefaultAsync<DbFileInfoData, ProjectFileInfo>(_files.Find(filter), token);
    }

    public virtual async Task<DatabaseFile[]> GetAllFiles(CancellationToken token)
        => await _contentManager.QueryFiles(token)
           .TakeWhile(_ => !token.IsCancellationRequested)
           .Select(
                d => new DatabaseFile(
                    new ProjectFileId(d.FileId),
                    new FileName(d.FileName),
                    new FileSize(d.Length),
                    new JobName(d.JobName)))
           .ToArrayAsync(token);

    public async Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
    {
        try
        {
            var filter = _files.Operations.Eq(d => d.Id, projectFile.Id.Value);

            if(await _files.CountEntrys(filter, token) == 1)
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
                token);

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

        foreach (TItem item in items)
        {
            string? result = await executor(item);

            if(string.IsNullOrWhiteSpace(result)) continue;

            errors = errors.Add(result);
        }

        return errors.IsEmpty ? string.Empty : string.Join($", {Environment.NewLine}", errors);
    }

    public async Task<string> CommitFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.CommitFile(id, token));

    public async Task<string> DeleteFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.DeleteFile(id, token));
}