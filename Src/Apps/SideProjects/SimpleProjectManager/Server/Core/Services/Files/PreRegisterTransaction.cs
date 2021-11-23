using Akkatecture.Jobs.Commands;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using Tauron.Application;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record PreRegistrationContext(Func<Stream> ToRegister, ProjectFileId FileId, GridFSBucket Bucket, TaskManagerCore TaskManager, CancellationToken Token);


public sealed class PreRegisterTransaction : SimpleTransaction<PreRegistrationContext>
{
    public PreRegisterTransaction(ILogger logger) : base(logger)
    {
        Register(AddToDatabase);
        Register(AddShortTimeAutoDelete);
    }

    private static async ValueTask<Rollback<PreRegistrationContext>> AddShortTimeAutoDelete(PreRegistrationContext context)
    {
        var id = FilePurgeId.For(context.FileId);
        var result = await context.TaskManager
           .AddNewTask(
                AddTaskCommand.Create(
                    "Auto SortTime File Delete",
                    new Schedule<FilePurgeJob, FilePurgeId>(id, new FilePurgeJob(context.FileId), DateTime.Now + TimeSpan.FromMinutes(30))),
                context.Token);

        if (!result.Ok)
            throw new InvalidOperationException("Task zu Automatischen Löschen der Datei konnte nicht erstellt werden");

        return async _ => await context.TaskManager.Delete(id.Value, default);
    }

    private static async ValueTask<Rollback<PreRegistrationContext>> AddToDatabase(PreRegistrationContext context)
    {
        var bucked = context.Bucket;
        var id = ObjectId.GenerateNewId();
        
        await using var stream = context.ToRegister();
        await bucked.UploadFromStreamAsync(id, context.FileId.Value, stream, cancellationToken:context.Token);

        return async _ => await bucked.DeleteAsync(id);
    }
}