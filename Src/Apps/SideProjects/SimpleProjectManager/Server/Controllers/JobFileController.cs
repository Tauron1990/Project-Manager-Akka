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

        
        [HttpPost]
        public ValueTask<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
            => _service.RegisterFile(projectFile, token);

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
        public async Task<string> UploadFiles(UploadFiles files, 
            [FromServices] FileContentManager contentManager,
            [FromServices] ICriticalErrorService errorService,
            [FromServices] ILogger<JobFileController> logger,
            CancellationToken cancellationToken)
        {
            var transaction = new FileUploadTransaction();
            var context = new FileUploadContext(files, contentManager, _service, cancellationToken);
            var errorHelper = new CriticalErrorHelper(nameof(JobFileController), errorService, logger);

            return await errorHelper.ProcessTransaction(
                await transaction.Execute(context),
                nameof(UploadFiles),
                () => ImmutableList<ErrorProperty>.Empty
                   .Add(new ErrorProperty("Job", files.JobName)))
                ?? string.Empty;
        }
    }
}
