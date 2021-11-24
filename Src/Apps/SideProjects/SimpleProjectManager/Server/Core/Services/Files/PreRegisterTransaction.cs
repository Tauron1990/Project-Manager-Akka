using Akkatecture.Jobs.Commands;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using Tauron.Application;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record PreRegistrationContext(Func<Stream> ToRegister, ProjectFileId FileId, string FileName, string JobName, GridFSBucket Bucket, TaskManagerCore TaskManager, CancellationToken Token);


public sealed class PreRegisterTransaction : SimpleTransaction<PreRegistrationContext>
{
    public PreRegisterTransaction()
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
        var meta = new BsonDocument
                   {
                       new BsonElement(FileContentManager.MetaJobName, context.JobName),
                       new BsonElement(FileContentManager.MetaFileNme, context.FileName)
                   };
        
        await using var stream = context.ToRegister();
        await bucked.UploadFromStreamAsync(id, context.FileId.Value, stream, new GridFSUploadOptions
                                                                             {
                                                                                 Metadata = meta
                                                                             }, context.Token);

        return async _ => await bucked.DeleteAsync(id);
    }
}