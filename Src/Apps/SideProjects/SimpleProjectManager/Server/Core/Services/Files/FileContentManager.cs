using System.Collections.Immutable;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class FileContentManager
{
    public const string MetaJobName = "JobName";
    public const string MetaFileNme = "FileName";
    
    private readonly IInternalFileRepository _bucked;
    private readonly TaskManagerCore _taskManager;
    private readonly IEventAggregator _aggregator;
    private readonly CommitRegistrationTransaction _commitRegistrationTransaction;
    private readonly PreRegisterTransaction _preRegisterTransaction;
    private readonly CriticalErrorHelper _criticalErrorHelper;
    
    public FileContentManager(IInternalFileRepository bucked, ICriticalErrorService criticalErrorService, TaskManagerCore taskManager, 
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

    public IAsyncEnumerable<FileEntry> QueryFiles(CancellationToken token)
        => _bucked.FindAllAsync(cancellationToken: token);

    public async ValueTask<string?> DeleteFile(ProjectFileId id, CancellationToken token)
    {
        return (await _criticalErrorHelper.Try(
            nameof(DeleteFile),
            async () =>
            {
                var search = await _bucked.FindByIdAsync(id.Value, token);

                if (search == null)
                {
                    _criticalErrorHelper.Logger.LogWarning("Die Datei {Id} wurde nicht gefunden", id.Value);
                    return OperationResult.Success();
                }

                await _bucked.DeleteAsync(search.Id, token);
                var result = await _taskManager.DeleteTask(FilePurgeId.For(id).Value, token);

                _aggregator.Publish(new FileDeleted(id));

                return result;
            }, token, 
            () => ImmutableList<ErrorProperty>
               .Empty.Add(new ErrorProperty("File Id", id.Value)))
            ).Error;
    }
}