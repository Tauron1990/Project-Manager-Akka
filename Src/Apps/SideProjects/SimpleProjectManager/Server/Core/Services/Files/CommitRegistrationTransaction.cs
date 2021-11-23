using Akkatecture.Jobs.Commands;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using Tauron.Application;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record CommitRegistrationContext(TaskManagerCore TaskManager, ProjectFileId Id, CancellationToken Token);


public sealed class CommitRegistrationTransaction : SimpleTransaction<CommitRegistrationContext>
{
    private Exception? Last;
    
    public CommitRegistrationTransaction(ILogger logger) : base(logger)
    {
        Register(CancelOldTask);
        Register(CreateCommitTask);
    }

    private async ValueTask<Rollback<CommitRegistrationContext>> CreateCommitTask(CommitRegistrationContext context)
    {
        var result = await context.TaskManager.AddNewTask(
            AddTaskCommand.Create(
                $"Datai {context.Id.Value} Löschen",
                new Schedule<FilePurgeJob, FilePurgeId>(
                    FilePurgeId.For(context.Id),
                    new FilePurgeJob(context.Id),
                    DateTime.Now + TimeSpan.FromDays(6 * 30))),
            context.Token);
        
        if(result.Ok) return _ => throw Last ?? new InvalidOperationException("Unbekannter Fehler");

        Last = CreateCommitExceptionFor("Erstellen des neun Tasks zum Automatischen Löschens");

        throw Last;
    }

    private async ValueTask<Rollback<CommitRegistrationContext>> CancelOldTask(CommitRegistrationContext context)
    {
        var (taskManagerCore, projectFileId, cancellationToken) = context;
        var result = await taskManagerCore.Delete(FilePurgeId.For(projectFileId).Value, cancellationToken);

        if (result.Ok) return _ => throw Last ?? new InvalidOperationException("Unbekannter Fehler");

        Last = CreateCommitExceptionFor("Abbrechen des Alten Tasks zum Automatischen Löschens");

        throw Last;

    }

    private static Exception CreateCommitExceptionFor(string step)
        => new InvalidOperationException($"Fehler bei der Finalen Fegistrierung beim {step}");
}