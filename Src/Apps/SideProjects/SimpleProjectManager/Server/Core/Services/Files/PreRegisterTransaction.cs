using Akkatecture.Jobs.Commands;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record PreRegistrationContext(Func<Stream> ToRegister, ProjectFileId FileId, string FileName, string JobName);


public sealed class PreRegisterTransaction : SimpleTransaction<PreRegistrationContext>
{
    private readonly GridFSBucket _bucket;
    private readonly TaskManagerCore _taskManager;

    public PreRegisterTransaction(GridFSBucket bucket, TaskManagerCore taskManager)
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
                    "Auto SortTime File Delete",
                    Schedule.Fixed(id, new FilePurgeJob(context.FileId), DateTime.Now + TimeSpan.FromMinutes(30))),
                token);

        if (!result.Ok)
            throw new InvalidOperationException("Task zu Automatischen Löschen der Datei konnte nicht erstellt werden");

        return async _ => await _taskManager.Delete(id.Value, default);
    }

    private async ValueTask<Rollback<PreRegistrationContext>> AddToDatabase(Context<PreRegistrationContext> transactionContext)
    {
        var (context, _, token) = transactionContext;
        
        var id = ObjectId.GenerateNewId();
        var meta = new BsonDocument
                   {
                       new BsonElement(FileContentManager.MetaJobName, context.JobName),
                       new BsonElement(FileContentManager.MetaFileNme, context.FileName)
                   };
        
        await using var stream = context.ToRegister();
        await _bucket.UploadFromStreamAsync(id, context.FileId.Value, stream, new GridFSUploadOptions
                                                                             {
                                                                                 Metadata = meta
                                                                             }, token);

        return async _ => await _bucket.DeleteAsync(id);
    }
}