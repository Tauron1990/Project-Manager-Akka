using System.Collections.Concurrent;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Controllers.FileUpload;

public sealed record FileUploadContext(UploadFiles Files, ConcurrentBag<ProjectFileId> Ids);

public class FileUploadTransaction : SimpleTransaction<FileUploadContext>
{
    private readonly FileContentManager _contentManager;
    private readonly IJobFileService _fileService;

    public FileUploadTransaction(FileContentManager contentManager, IJobFileService fileService)
    {
        _contentManager = contentManager;
        _fileService = fileService;
        RegisterLoop(c => c.Data.Files.Files, UploadTempFile);
    }

    private async ValueTask<Rollback<FileUploadContext>> UploadTempFile(Context<FileUploadContext> transactionContext, IFormFile file)
    {
        if(file.Length > MaxSize.MaxFileSize)
            throw new InvalidOperationException($"Die Datei {file.FileName} ist zu groß");
        if(FilesState.AllowedContentTypes.All(s => new FileMime(file.ContentType) != s))
            throw new InvalidOperationException($"Die Datei {file.FileName} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt");

        ((UploadFiles files, var ids), _, CancellationToken token) = transactionContext;

        string name = file.FileName;
        var projectName = new ProjectName(files.JobName);

        ProjectFileId id = ProjectFileId.For(projectName, new FileName(name));

        string? preRegister = await _contentManager.PreRegisterFile(file.OpenReadStream, id, name, files.JobName, token);

        if(!string.IsNullOrWhiteSpace(preRegister)) throw new InvalidOperationException(preRegister);

        SimpleResult result = await _fileService.RegisterFile(
            new ProjectFileInfo(
                id,
                projectName,
                new FileName(name),
                new FileSize(file.Length),
                FileType.OtherFile,
                new FileMime(file.ContentType)),
            token);

        if(result.IsError())
            throw result.GetException();

        ids.Add(id);

        return async _ => await _contentManager.DeleteFile(id, default);
    }
}