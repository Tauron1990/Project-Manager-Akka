using System.Collections.Immutable;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class FileContentManager
{
    public const string MetaJobName = "JobName";
    public const string MetaFileNme = "FileName";
    
    private readonly GridFSBucket _bucked;
    private readonly TaskManagerCore _taskManager;
    private readonly IEventAggregator _aggregator;
    private readonly CommitRegistrationTransaction _commitRegistrationTransaction;
    private readonly PreRegisterTransaction _preRegisterTransaction;
    private readonly CriticalErrorHelper _criticalErrorHelper;
    
    public FileContentManager(GridFSBucket bucked, ICriticalErrorService criticalErrorService, TaskManagerCore taskManager, 
        ILogger<FileContentManager> logger, IEventAggregator aggregator,
        CommitRegistrationTransaction commitRegistrationTransaction, PreRegisterTransaction preRegisterTransaction)
    {
        _bucked = bucked;
        _taskManager = taskManager;
        _aggregator = aggregator;
        _commitRegistrationTransaction = commitRegistrationTransaction;
        _preRegisterTransaction = preRegisterTransaction;
        _criticalErrorHelper = new CriticalErrorHelper(nameof(FileContentManager), criticalErrorService, logger);
    }

    public async ValueTask<string?> PreRegisterFile(Func<Stream> toRegister, ProjectFileId id, string fileName, string jobName, CancellationToken token)
    {
        var context = new PreRegistrationContext(toRegister, id, fileName, jobName);

        var result = await _criticalErrorHelper.ProcessTransaction(
            await _preRegisterTransaction.Execute(context, token),
            nameof(PreRegisterFile),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("File Id", id.Value))
               .Add(new ErrorProperty("File Name", fileName)));

        if(string.IsNullOrWhiteSpace(result))
            _aggregator.Publish(FileAdded.Inst);
        
        return result;
    }

    public async ValueTask<string?> CommitFile(ProjectFileId id, CancellationToken token)
    {
        return await _criticalErrorHelper.ProcessTransaction(
            await _commitRegistrationTransaction.Execute(id, token),
            nameof(CommitFile),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("File Id", id.Value)));
    }

    public async ValueTask<IAsyncCursor<GridFSFileInfo>> QueryFiles(CancellationToken token)
        => await _bucked.FindAsync(Builders<GridFSFileInfo>.Filter.Empty, cancellationToken: token);

    public async ValueTask<string?> DeleteFile(ProjectFileId id, CancellationToken token)
    {
        return (await _criticalErrorHelper.Try(
            nameof(DeleteFile),
            async () =>
            {
                var filter = Builders<GridFSFileInfo>.Filter.Eq(m => m.Filename, id.Value);
                var search = await (await _bucked.FindAsync(filter, cancellationToken: token)).FirstOrDefaultAsync(token);

                if (search == null)
                {
                    _criticalErrorHelper.Logger.LogWarning("Die Datei {Id} wurde nicht gefunden", id.Value);
                    return OperationResult.Success();
                }

                await _bucked.DeleteAsync(search.Id, token);
                var result = await _taskManager.Delete(FilePurgeId.For(id).Value, token);

                _aggregator.Publish(new FileDeleted(id));

                return result;
            }, token, 
            () => ImmutableList<ErrorProperty>
               .Empty.Add(new ErrorProperty("File Id", id.Value)))
            ).Error;
    }
}