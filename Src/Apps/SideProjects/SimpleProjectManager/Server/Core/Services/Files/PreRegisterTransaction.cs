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
        var (context, _, token) = transactionContext;
        
        var id = FilePurgeId.For(context.FileId);
        var result = await _taskManager
           .AddNewTask(
                AddTaskCommand.Create(
                    "Automatisches Löschen der Datei - 30 Minuten",
                    Schedule.Fixed(id, new FilePurgeJob(context.FileId), DateTime.Now + TimeSpan.FromMinutes(30))),
                token);

        if (!result.Ok)
            throw new InvalidOperationException("Task zu Automatischen Löschen der Datei konnte nicht erstellt werden");

        return async _ => await _taskManager.Delete(id.Value, default);
    }

    private async ValueTask<Rollback<PreRegistrationContext>> AddToDatabase(Context<PreRegistrationContext> transactionContext)
    {
        var (context, _, token) = transactionContext;
        
        var id = ObjectId.GenerateNewId().ToString();
        
        await using var stream = context.ToRegister();
        await _bucket.UploadFromStreamAsync(id, context.FileId.Value, stream, context.JobName, context.FileName, token);

        return async _ => await _bucket.DeleteAsync(id, token);
    }
}