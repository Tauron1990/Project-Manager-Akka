using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers
{
    [ApiController, JsonifyErrors, Route(ApiPaths.FilesApi + "/[action]")]
    public class JobFileController : Controller, IJobFileService
    {
        private readonly IJobFileService _service;

        public JobFileController(IJobFileService service)
            => _service = service;

        [Publish, HttpGet]
        public Task<ProjectFileInfo?> GetJobFileInfo([FromQuery] ProjectFileId id, CancellationToken token)
            => _service.GetJobFileInfo(id, token);

        [Publish, HttpGet]
        public Task<DatabaseFile[]> GetAllFiles(CancellationToken token)
            => _service.GetAllFiles(token);


        [HttpPost]
        public Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
            => _service.RegisterFile(projectFile, token);

        public Task<string> CommitFiles(FileList files, CancellationToken token)
            => _service.CommitFiles(files, token);

        public Task<string> DeleteFiles(FileList files, CancellationToken token)
            => _service.DeleteFiles(files, token);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileDownload(
            [FromRoute] ProjectFileId id, [FromServices]IInternalDataRepository dataRepository, [FromServices] IInternalFileRepository bucked, CancellationToken token)
        {
            var coll = dataRepository.Databases.FileInfos;
            var filter = coll.Operations.Eq(p => p.Id, id.Value);
            var data = await coll.Find(filter).FirstOrDefaultAsync(token);
            if(data == null) return NotFound();

            string fileEntry = bucked.FindIdByFileName(data.Id).First();

            return File(
                await bucked.OpenStream(fileEntry, token),
                data.Mime, data.FileName);
        }
        
        [HttpPost] 
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = 800 * 1024 * 1024)]
        public async Task<UploadFileResult> UploadFiles(
            [FromForm]UploadFiles files, 
            [FromServices] FileUploadTransaction transaction,
            [FromServices] ICriticalErrorService errorService,
            [FromServices] ILogger<JobFileController> logger,
            CancellationToken cancellationToken)
        {
            var ids = new ConcurrentBag<ProjectFileId>();
            var context = new FileUploadContext(files, ids);
            var errorHelper = new CriticalErrorHelper(nameof(JobFileController), errorService, logger);

            var result = await errorHelper.ProcessTransaction(
                             await transaction.Execute(context, cancellationToken),
                             nameof(UploadFiles),
                             () => ImmutableList<ErrorProperty>.Empty
                                .Add(new ErrorProperty("Job", files.JobName)))
                      ?? string.Empty;

            return string.IsNullOrWhiteSpace(result) 
                ? new UploadFileResult(result, ids.ToImmutableList())
                : new UploadFileResult(result, ImmutableList<ProjectFileId>.Empty);
        }
    }
}
