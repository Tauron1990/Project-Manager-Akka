using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Core.Services;

public class JobFileService : IJobFileService
{
    private readonly IMongoCollection<FileInfoData> _files;

    public JobFileService(InternalDataRepository dataRepository)
        => _files = dataRepository.Collection<FileInfoData>();

    public async Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
    {
        var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, id);
        var result = await _files.Find(filter).FirstOrDefaultAsync(token);

        return result == null 
            ? null 
            : new ProjectFileInfo(result.Id, result.ProjectName, result.FileName, result.Size, result.FileType, result.Mime);
    }

    public async Task<ApiResult> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
    {
        try
        {
            var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, projectFile.Id);

            if (await _files.CountDocumentsAsync(filter, cancellationToken: token) == 1)
                return new ApiResult("Der eintrag existiert schon");

            await _files.InsertOneAsync(
                new FileInfoData
                {
                    Id = projectFile.Id,
                    ProjectName = projectFile.ProjectName,
                    FileName = projectFile.FileName,
                    Size = projectFile.Size,
                    FileType = projectFile.FileType,
                    Mime = projectFile.Mime
                },
                cancellationToken: token);

            return new ApiResult(string.Empty);
        }
        catch (Exception ex)
        {
            return new ApiResult(ex.Message);
        }
    }
}