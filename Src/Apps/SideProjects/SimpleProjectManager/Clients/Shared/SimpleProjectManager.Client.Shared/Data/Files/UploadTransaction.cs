﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services;
using Tauron;
using Tauron.Application;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.Files;

public sealed record UploadTransactionContext(ImmutableList<FileUploadFile> Files, ProjectName JobName);

public sealed class UploadTransaction : SimpleTransaction<UploadTransactionContext>
{
    private readonly HttpClient _client;
    private readonly IJobDatabaseService _databaseService;
    private readonly IJobFileService _fileService;

    public UploadTransaction(HttpClient client, IJobFileService fileService, IJobDatabaseService databaseService)
    {
        _client = client;

        _fileService = fileService;
        _databaseService = databaseService;

        Register(UploadFiles);
        Register(UpdateProjectData);
        Register(CommitFiles);
    }

    private async ValueTask<Rollback<UploadTransactionContext>> CommitFiles(Context<UploadTransactionContext> context)
    {
        Console.WriteLine("Commit Files");

        SimpleResult result = await TimeoutToken.WithDefault(
            context.Token,
            t => _fileService.CommitFiles(new FileList(context.Metadata.Get<ImmutableList<ProjectFileId>>()), t));

        return result.ThrowIfFail<Rollback<UploadTransactionContext>>(() => _ => default);
    }

    private async ValueTask<Rollback<UploadTransactionContext>> UpdateProjectData(Context<UploadTransactionContext> transactionContext)
    {
        ((_, ProjectName projectName), ContextMetadata meta, CancellationToken token) = transactionContext;
        ProjectId id = ProjectId.For(projectName);
        meta.Set(id);

        (SimpleResult failMessage, bool isNew) = await TimeoutToken.WithDefault(
            token,
            t => _databaseService.AttachFiles(new ProjectAttachFilesCommand(id, projectName, meta.Get<ImmutableList<ProjectFileId>>()), t));

        Console.WriteLine("Files Attached");

        return failMessage.ThrowIfFail<Rollback<UploadTransactionContext>>(
            () => async c =>
                  {
                      (_, ContextMetadata? contextMetadata, CancellationToken cancellationToken) = c;
                      SimpleResult deleteResult;

                      if(!isNew)
                          deleteResult = await TimeoutToken.WithDefault(
                              cancellationToken,
                              t => _databaseService.RemoveFiles(
                                  new ProjectRemoveFilesCommand(
                                      contextMetadata.Get<ProjectId>(),
                                      contextMetadata.Get<ImmutableList<ProjectFileId>>()),
                                  t));
                      else
                          deleteResult = await TimeoutToken.WithDefault(
                              cancellationToken,
                              t => _databaseService.DeleteJob(contextMetadata.Get<ProjectId>(), t));

                      deleteResult.ThrowIfFail();
                  });
    }

    private async ValueTask<Rollback<UploadTransactionContext>> UploadFiles(Context<UploadTransactionContext> transactionContext)
    {
        ((var fileUploadFiles, ProjectName projectName), ContextMetadata meta, CancellationToken token) = transactionContext;
        fileUploadFiles.ForEach(f => f.UploadState = UploadState.Uploading);

        var requestContent = new MultipartFormDataContent();

        #if DEBUG
        Console.WriteLine($"Upload JobName = {projectName.Value}");
        #endif

        requestContent.Add(new StringContent(projectName.Value, null), "JobName");

        (SimpleResult failMessage, var ids) = await TimeoutToken.With(
            TimeSpan.FromMinutes(30),
            token,
            async t =>
            {
                foreach (FileUploadFile file in fileUploadFiles)
                {
                    t.ThrowIfCancellationRequested();

                    Stream stream = file.Open(t);

                    Console.WriteLine("Add File Stream");
                    requestContent.Add(
                        new StreamContent(stream)
                        {
                            Headers =
                            {
                                ContentLength = file.UploadFile.Size.Value,
                                ContentType = new MediaTypeHeaderValue(file.ContentType.Value)
                            }
                        },
                        "files",
                        file.Name.Value);
                }

                #if DEBUG
                Console.WriteLine("Upload Files");
                #endif

                return await _client.PostJson<UploadFileResult>($"{ApiPaths.FilesApi}/{nameof(IJobFileServiceDef.UploadFiles)}", requestContent, t)
                    ?? new UploadFileResult(SimpleResult.Failure("Unbekannter Fehler"), ImmutableList<ProjectFileId>.Empty);
            });

        meta.Set(ids);

        return failMessage.ThrowIfFail<Rollback<UploadTransactionContext>>(
            () => async c =>
                  {
                      SimpleResult deleteResult = await TimeoutToken.WithDefault(
                          c.Token,
                          t => _fileService.DeleteFiles(new FileList(c.Metadata.Get<ImmutableList<ProjectFileId>>()), t));

                      deleteResult.ThrowIfFail();
                  });
    }
}