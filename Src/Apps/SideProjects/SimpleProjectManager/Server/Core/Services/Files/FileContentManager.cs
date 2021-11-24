using System.Collections.Immutable;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class FileContentManager
{
    public const string MetaJobName = "JobName";
    public const string MetaFileNme = "FileName";
    
    private readonly GridFSBucket _bucked;
    private readonly TaskManagerCore _taskManager;
    private readonly IEventAggregator _aggregator;
    private readonly CriticalErrorHelper _criticalErrorHelper;
    
    public FileContentManager(GridFSBucket bucked, ICriticalErrorService criticalErrorService, TaskManagerCore taskManager, 
        ILogger<FileContentManager> logger, IEventAggregator aggregator)
    {
        _bucked = bucked;
        _taskManager = taskManager;
        _aggregator = aggregator;
        _criticalErrorHelper = new CriticalErrorHelper(nameof(FileContentManager), criticalErrorService, logger);
    }

    public async ValueTask<string?> PreRegisterFile(Func<Stream> toRegister, ProjectFileId id, string fileName, string jobName, CancellationToken token)
    {
        var transaction = new PreRegisterTransaction();
        var context = new PreRegistrationContext(toRegister, id, fileName, jobName, _bucked, _taskManager, token);

        var result = await _criticalErrorHelper.ProcessTransaction(
            await transaction.Execute(context),
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
        var trans = new CommitRegistrationTransaction();
        var context = new CommitRegistrationContext(_taskManager, id, token);

        return await _criticalErrorHelper.ProcessTransaction(
            await trans.Execute(context),
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

                if (search == null) return OperationResult.Failure("Datei nicht gefunden");

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