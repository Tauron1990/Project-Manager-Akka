﻿using Akkatecture.Jobs.Commands;
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
        (ProjectFileId projectFileId, ContextMetadata meta, CancellationToken token) = transactionContext;

        SimpleResult result = await _taskManagerCore.AddNewTask(
            AddTaskCommand.Create(
                $"Datei {projectFileId.Value} Löschen - 6 Monate",
                new Schedule<FilePurgeJob, FilePurgeId>(
                    FilePurgeId.For(projectFileId),
                    new FilePurgeJob(projectFileId),
                    DateTime.Now + TimeSpan.FromDays(6 * 30))),
            token).ConfigureAwait(false);

        if(result.IsSuccess()) return c => throw c.Metadata.GetOptional<Exception>() ?? new InvalidOperationException("Unbekannter Fehler");

        Exception error = CreateCommitExceptionFor("Erstellen des neun Tasks zum Automatischen Löschens");
        meta.Set(error);

        throw error;
    }

    private async ValueTask<Rollback<ProjectFileId>> CancelOldTask(Context<ProjectFileId> transactionContext)
    {
        (ProjectFileId projectFileId, ContextMetadata meta, CancellationToken cancellationToken) = transactionContext;
        SimpleResult result = await _taskManagerCore.DeleteTask(FilePurgeId.For(projectFileId).Value, cancellationToken).ConfigureAwait(false);

        if(result.IsSuccess()) return c => throw c.Metadata.GetOptional<Exception>() ?? new InvalidOperationException("Unbekannter Fehler");

        Exception error = CreateCommitExceptionFor("Abbrechen des Alten Tasks zum Automatischen Löschens");
        meta.Set(error);

        throw error;

    }

    private static Exception CreateCommitExceptionFor(string step)
        => new InvalidOperationException($"Fehler bei der Finalen Fegistrierung beim {step}");
}