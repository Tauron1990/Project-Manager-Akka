using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Shared;
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
        public ValueTask<ProjectFileInfo?> GetJobFileInfo([FromQuery]ProjectFileId id, CancellationToken token)
            => _service.GetJobFileInfo(id, token);

        [Publish, HttpGet]
        public ValueTask<DatabaseFile[]> GetAllFiles(CancellationToken token)
            => _service.GetAllFiles(token);


        [HttpPost]
        public ValueTask<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
            => _service.RegisterFile(projectFile, token);

        public ValueTask<string> CommitFiles(FileList files, CancellationToken token)
            => _service.CommitFiles(files, token);

        public ValueTask<string> DeleteFiles(FileList files, CancellationToken token)
            => _service.DeleteFiles(files, token);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileDownload(
            [FromRoute] ProjectFileId id, [FromServices]InternalDataRepository repository, [FromServices] GridFSBucket bucked, CancellationToken token)
        {
            var coll = repository.Collection<FileInfoData>();
            var filter = Builders<FileInfoData>.Filter.Eq(p => p.Id, id);
            var data = await coll.Find(filter).FirstOrDefaultAsync(token);
            if(data == null) return NotFound();

            
            return File(
                await bucked.OpenDownloadStreamByNameAsync(data.Id.Value, cancellationToken:token),
                data.Mime.Value, data.FileName.Value);
        }
        
        [HttpPost]
        public async Task<UploadFileResult> UploadFiles(UploadFiles files, 
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
