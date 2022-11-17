using Akkatecture.Jobs.Commands;
using MongoDB.Bson;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record PreRegistrationContext(Func<Stream> ToRegister, ProjectFileId FileId, string FileName, string JobName);

public sealed class PreRegisterTransaction : SimpleTransaction<PreRegistrationContext>
{
    private readonly IInternalFileRepository _bucket;
    private readonly TaskManagerCore _taskManager;

    public PreRegisterTransaction(IInternalFileRepository bucket, TaskManagerCore taskManager)
    {
        _bucket = bucket;
        _taskManager = taskManager;

        Register(AddToDatabase);
        Register(AddShortTimeAutoDelete);
    }

    private async ValueTask<Rollback<PreRegistrationContext>> AddShortTimeAutoDelete(Context<PreRegistrationContext> transactionContext)
    {
        PreRegistrationContext context = transactionContext.Data;
        CancellationToken token = transactionContext.Token;

        FilePurgeId id = FilePurgeId.For(context.FileId);
        SimpleResult result = await _taskManager
           .AddNewTask(
                AddTaskCommand.Create(
                    "Automatisches Löschen der Datei - 30 Minuten",
                    Schedule.Fixed(id, new FilePurgeJob(context.FileId), DateTime.Now + TimeSpan.FromMinutes(30))),
                token).ConfigureAwait(false);

        if(result.IsError())
            throw new InvalidOperationException("Task zu Automatischen Löschen der Datei konnte nicht erstellt werden");

        return async _ => await _taskManager.DeleteTask(id.Value, default).ConfigureAwait(false);
    }

    private async ValueTask<Rollback<PreRegistrationContext>> AddToDatabase(Context<PreRegistrationContext> transactionContext)
    {
        PreRegistrationContext context = transactionContext.Data;
        CancellationToken token = transactionContext.Token;


        var id = ObjectId.GenerateNewId().ToString();

        Stream stream = context.ToRegister();
        await using (stream.ConfigureAwait(false))
        {
            await _bucket.UploadFromStreamAsync(id, context.FileId.Value, stream, context.JobName, context.FileName, token).ConfigureAwait(false);

            return async _ => await _bucket.DeleteAsync(id, token).ConfigureAwait(false);
        }
    }
}