using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Core.Services;

public class JobFileService : IJobFileService
{
    private readonly ILogger<JobFileService> _logger;
    private readonly IMongoCollection<FileInfoData> _files;

    public JobFileService(InternalDataRepository dataRepository, ILogger<JobFileService> logger)
    {
        _logger = logger;
        _files = dataRepository.Collection<FileInfoData>();
    }

    public async Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
    {
        var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, id);
        var result = await _files.Find(filter).FirstOrDefaultAsync(token);

        return result == null 
            ? null 
            : new ProjectFileInfo(result.Id, result.ProjectName, result.FileName, result.Size, result.FileType, result.Mime);
    }

    public async Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
    {
        try
        {
            var filter = Builders<FileInfoData>.Filter.Eq(d => d.Id, projectFile.Id);

            if (await _files.CountDocumentsAsync(filter, cancellationToken: token) == 1)
                return "Der eintrag existiert schon";

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

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on Registrationg File");
            return ex.Message;
        }
    }
}