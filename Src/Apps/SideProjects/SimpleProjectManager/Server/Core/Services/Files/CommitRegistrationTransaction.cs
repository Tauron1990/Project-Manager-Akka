using Akkatecture.Jobs.Commands;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class CommitRegistrationTransaction : SimpleTransaction<ProjectFileId>
{
    private readonly TaskManagerCore _taskManagerCore;

    public CommitRegistrationTransaction(TaskManagerCore taskManagerCore)
    {
        _taskManagerCore = taskManagerCore;
        
        Register(CancelOldTask);
        Register(CreateCommitTask);
    }

    private async ValueTask<Rollback<ProjectFileId>> CreateCommitTask(Context<ProjectFileId> transactionContext)
    {
        var (projectFileId, meta, token) = transactionContext;
        
        var result = await _taskManagerCore.AddNewTask(
            AddTaskCommand.Create(
                $"Datai {projectFileId.Value} Löschen",
                new Schedule<FilePurgeJob, FilePurgeId>(
                    FilePurgeId.For(projectFileId),
                    new FilePurgeJob(projectFileId),
                    DateTime.Now + TimeSpan.FromDays(6 * 30))),
            token);
        
        if(result.Ok) return c => throw c.Metadata.GetOptional<Exception>() ??  new InvalidOperationException("Unbekannter Fehler");

        var error = CreateCommitExceptionFor("Erstellen des neun Tasks zum Automatischen Löschens");
        meta.Set(error);
        
        throw error;
    }

    private async ValueTask<Rollback<ProjectFileId>> CancelOldTask(Context<ProjectFileId> transactionContext)
    {
        var (projectFileId, meta, cancellationToken) = transactionContext;
        var result = await _taskManagerCore.Delete(FilePurgeId.For(projectFileId).Value, cancellationToken);

        if (result.Ok) return c => throw c.Metadata.GetOptional<Exception>() ?? new InvalidOperationException("Unbekannter Fehler");

        var error = CreateCommitExceptionFor("Abbrechen des Alten Tasks zum Automatischen Löschens");
        meta.Set(error);
        
        throw error;

    }

    private static Exception CreateCommitExceptionFor(string step)
        => new InvalidOperationException($"Fehler bei der Finalen Fegistrierung beim {step}");
}