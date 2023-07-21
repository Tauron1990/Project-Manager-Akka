using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[JsonifyErrors]
[Route(ApiPaths.FilesApi + "/[action]")]
public class JobFileController : Controller, IJobFileService
{
    private readonly IJobFileService _service;

    public JobFileController(IJobFileService service)
        => _service = service;

    [HttpGet]
    public Task<ProjectFileInfo?> GetJobFileInfo([FromQuery] ProjectFileId id, CancellationToken token)
        => _service.GetJobFileInfo(id, token);

    [HttpGet]
    public Task<DatabaseFile[]> GetAllFiles(CancellationToken token)
        => _service.GetAllFiles(token);


    [HttpPost]
    public Task<SimpleResult> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
        => _service.RegisterFile(projectFile, token);

    public Task<SimpleResult> CommitFiles(FileList files, CancellationToken token)
        => _service.CommitFiles(files, token);

    public Task<SimpleResult> DeleteFiles(FileList files, CancellationToken token)
        => _service.DeleteFiles(files, token);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFileDownload(
        [FromRoute] ProjectFileId id, [FromServices] IInternalDataRepository dataRepository, [FromServices] IInternalFileRepository bucked, CancellationToken token)
    {
        var coll = dataRepository.Databases.FileInfos;
        var filter = coll.Operations.Eq(p => p.Id, id.Value);
        DbFileInfoData? data = await coll.Find(filter).FirstOrDefaultAsync(token);

        if(data == null) return NotFound();

        string fileEntry = bucked.FindIdByFileName(data.Id).First();

        return File(
            await bucked.OpenStream(fileEntry, token),
            data.Mime,
            data.FileName);
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 800 * 1024 * 1024)]
    public async Task<UploadFileResult> UploadFiles(
        [FromForm] UploadFiles files,
        [FromServices] FileUploadTransaction transaction,
        [FromServices] ICriticalErrorService errorService,
        [FromServices] ILogger<JobFileController> logger,
        CancellationToken cancellationToken)
    {
        var ids = new ConcurrentBag<ProjectFileId>();
        var context = new FileUploadContext(files, ids);
        var errorHelper = new CriticalErrorHelper(nameof(JobFileController), errorService, logger);

        SimpleResult result = await errorHelper.ProcessTransaction(
            await transaction.Execute(context, cancellationToken),
            nameof(UploadFiles),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty(PropertyName.From("Job"), PropertyValue.From(files.JobName))));

        return result.IsError()
            ? new UploadFileResult(result, ImmutableList<ProjectFileId>.Empty)
            : new UploadFileResult(result, ids.ToImmutableList());
    }
}