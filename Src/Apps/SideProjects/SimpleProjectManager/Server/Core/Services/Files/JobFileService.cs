﻿using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Akkatecture.Jobs;
using AutoMapper;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Services;

public class JobFileService : IJobFileService, IDisposable
{
    private readonly ILogger<JobFileService> _logger;
    private readonly FileContentManager _contentManager;
    private readonly IDatabaseCollection<DbFileInfoData> _files;
    private readonly IDisposable _subscription;
    private readonly IMapper _mapper;

    public JobFileService(IInternalDataRepository dataRepository, ILogger<JobFileService> logger, IEventAggregator aggregator, FileContentManager contentManager)
    {
        _logger = logger;
        _contentManager = contentManager;
        _files = dataRepository.Databases.FileInfos;
        _mapper = dataRepository.Databases.Mapper;
        
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

                                        var filter = _files.Operations.Eq(m => m.Id, d.Id.Value);
                                        DbOperationResult result = _files.DeleteOne(filter);

                                        if (!result.IsAcknowledged || result.DeletedCount != 1) return;

                                        using (Computed.Invalidate())
                                            GetJobFileInfo(d.Id, default).Ignore();
                                    })
                        };
    }

    public virtual async Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
    {
        if (Computed.IsInvalidating()) return null;
        
        var filter =_files.Operations.Eq(d => d.Id, id.Value);
        var result = await _files.Find(filter).FirstOrDefaultAsync(token);

        return result == null 
            ? null 
            : new ProjectFileInfo(result.Id, result.ProjectName, result.FileName, result.Size, result.FileType, result.Mime);
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
           .ToArrayAsync(cancellationToken: token);

    public async Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
    {
        try
        {
            var filter = _files.Operations.Eq(d => d.Id, projectFile.Id);

            if (await _files.CountEntrys(filter, token) == 1)
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

    public async Task<string> CommitFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.CommitFile(id, token));

    public async Task<string> DeleteFiles(FileList files, CancellationToken token)
        => await AggregateErrors(files.Files, id => _contentManager.DeleteFile(id, token));

    public void Dispose()
        => _subscription.Dispose();
}