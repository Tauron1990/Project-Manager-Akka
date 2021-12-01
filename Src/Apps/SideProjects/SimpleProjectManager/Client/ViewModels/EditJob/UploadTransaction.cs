using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Reactive;
using SimpleProjectManager.Client.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed record UploadTransactionContext(ImmutableList<FileUploadFile> Files, ProjectName JobName);

public sealed class UploadTransaction : SimpleTransaction<UploadTransactionContext>
{
    private readonly IEventAggregator _aggregator;
    private readonly HttpClient _client;
    private readonly IJobFileService _fileService;
    private readonly IJobDatabaseService _databaseService;

    public UploadTransaction(IEventAggregator aggregator, HttpClient client, IJobFileService fileService, IJobDatabaseService databaseService)
    {
        _aggregator = aggregator;
        _client = client;

        _fileService = fileService;
        _databaseService = databaseService;
        
        Register(UploadFiles);
        Register(UpdateProjectData);
        Register(CommitFiles);
    }

    private async ValueTask<Rollback<UploadTransactionContext>> CommitFiles(Context<UploadTransactionContext> context)
    {
        var result = await TimeoutToken.WithDefault(context.Token,
            t => _fileService.CommitFiles(new FileList(context.Metadata.Get<ImmutableList<ProjectFileId>>()), t));

        return result.ThrowIfFail<Rollback<UploadTransactionContext>>(() => _ => default);
    }

    private async ValueTask<Rollback<UploadTransactionContext>> UpdateProjectData(Context<UploadTransactionContext> transactionContext)
    {
        var ((_, projectName), meta, token) = transactionContext;
        var id = ProjectId.For(projectName);
        meta.Set(id);
        
        var (failMessage, isNew) = await TimeoutToken.WithDefault(token,
            t => _databaseService.AttachFiles(new ProjectAttachFilesCommand(id, projectName, meta.Get<ImmutableList<ProjectFileId>>()), t));

        return failMessage.ThrowIfFail<Rollback<UploadTransactionContext>>(
            () => async c =>
                  {
                      if(!isNew) return;

                      var deleteResult = await TimeoutToken.WithDefault(c.Token,
                          t => _databaseService.DeleteJob(c.Metadata.Get<ProjectId>(), t));

                      deleteResult.ThrowIfFail();
                  });
    }

    private async ValueTask<Rollback<UploadTransactionContext>> UploadFiles(Context<UploadTransactionContext> transactionContext)
    {
        var ((fileUploadFiles, projectName), meta, token) = transactionContext;
        fileUploadFiles.ForEach(f => f.UploadState = UploadState.Uploading);

        var requestContent = new MultipartFormDataContent();

        #if DEBUG
        Console.WriteLine($"Upload JobName = {projectName.Value}");
        #endif

        requestContent.Add(new StringContent(projectName.Value, null), "JobName");

        var (failMessage, ids) = await TimeoutToken.With(
            TimeSpan.FromMinutes(30),
            token,
            async t =>
            {
                foreach (var file in fileUploadFiles)
                {
                    t.ThrowIfCancellationRequested();

                    var stream = file.Open(t)
                       .Recover(e => _aggregator.PublishError(e));

                    if (stream.IsSuccess)
                    {
                        requestContent.Add(
                            new StreamContent(stream.Get())
                            {
                                Headers =
                                {
                                    ContentLength = file.BrowserFile.Size,
                                    ContentType = new MediaTypeHeaderValue(file.ContentType)
                                }
                            },
                            "files",
                            file.Name);
                    }
                }

                #if DEBUG
                Console.WriteLine("Upload Files");
                #endif

                return await _client.PostJson<UploadFileResult>($"{ApiPaths.FilesApi}/{nameof(IJobFileServiceDef.UploadFiles)}", requestContent, t)
                    ?? new UploadFileResult("Unbekannter Fehler", ImmutableList<ProjectFileId>.Empty);
            });

        meta.Set(ids);
        
        return failMessage.ThrowIfFail<Rollback<UploadTransactionContext>>(
            () => async c =>
            {
                var deleteResult = await TimeoutToken.WithDefault(c.Token,
                    t => _fileService.DeleteFiles(new FileList(c.Metadata.Get<ImmutableList<ProjectFileId>>()), t));

                deleteResult.ThrowIfFail();
            });
    }
}