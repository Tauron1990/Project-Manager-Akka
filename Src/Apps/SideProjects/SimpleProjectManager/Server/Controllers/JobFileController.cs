using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
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


        public Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
            => _service.GetJobFileInfo(id, token);

        public Task<ApiResult> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
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
                await bucked.OpenDownloadStreamAsync(BsonValue.Create(data.Id.Value), cancellationToken:token),
                data.Mime.Value, data.FileName.Value);
        }
    }
}
