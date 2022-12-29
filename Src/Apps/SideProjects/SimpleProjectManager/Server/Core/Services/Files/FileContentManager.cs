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
    private readonly IEventAggregator _aggregator;

    private readonly IInternalFileRepository _bucked;
    private readonly CommitRegistrationTransaction _commitRegistrationTransaction;
    private readonly CriticalErrorHelper _criticalErrorHelper;
    private readonly PreRegisterTransaction _preRegisterTransaction;
    private readonly TaskManagerCore _taskManager;

    public FileContentManager(
        IInternalFileRepository bucked, ICriticalErrorService criticalErrorService, TaskManagerCore taskManager,
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

    public async ValueTask<SimpleResult> PreRegisterFile(Func<Stream> toRegister, ProjectFileId id, string fileName, string jobName, CancellationToken token)
    {
        var context = new PreRegistrationContext(toRegister, id, fileName, jobName);

        SimpleResult result = await _criticalErrorHelper.ProcessTransaction(
                await _preRegisterTransaction.Execute(context, token).ConfigureAwait(false),
                nameof(PreRegisterFile),
                () => ImmutableList<ErrorProperty>.Empty
                   .Add(new ErrorProperty(PropertyName.From("File Id"), PropertyValue.From(id.Value)))
                   .Add(new ErrorProperty(PropertyName.From("File Name"), PropertyValue.From(fileName))))
           .ConfigureAwait(false);

        if(result.IsSuccess())
            _aggregator.Publish(FileAdded.Inst);

        return result;
    }

    public async ValueTask<SimpleResult> CommitFile(ProjectFileId id, CancellationToken token)
    {
        return await _criticalErrorHelper.ProcessTransaction(
            await _commitRegistrationTransaction.Execute(id, token).ConfigureAwait(false),
            nameof(CommitFile),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty(PropertyName.From("File Id"), PropertyValue.From(id.Value)))).ConfigureAwait(false);
    }

    public IAsyncEnumerable<FileEntry> QueryFiles(CancellationToken token)
        => _bucked.FindAllAsync(token);

    public async ValueTask<SimpleResult> DeleteFile(ProjectFileId id, CancellationToken token)
    {
        return await _criticalErrorHelper.Try(
                nameof(DeleteFile),
                async () =>
                {
                    FileEntry? search = await _bucked.FindByIdAsync(id.Value, token).ConfigureAwait(false);

                    if(search is null)
                    {
                        _criticalErrorHelper.Logger.LogWarning("Die Datei {Id} wurde nicht gefunden", id.Value);

                        return SimpleResult.Success();
                    }

                    await _bucked.DeleteAsync(search.Id, token).ConfigureAwait(false);
                    SimpleResult result = await _taskManager.DeleteTask(FilePurgeId.For(id).Value, token).ConfigureAwait(false);

                    _aggregator.Publish(new FileDeleted(id));

                    return result;
                },
                token,
                () => ImmutableList<ErrorProperty>
                   .Empty.Add(new ErrorProperty(PropertyName.From("File Id"), PropertyValue.From(id.Value))))
           .ConfigureAwait(false);
    }
}