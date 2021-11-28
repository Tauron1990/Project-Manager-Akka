using System.Collections.Concurrent;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application;

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
        if(file.Length > FileUploaderViewModel.MaxSize)
            throw new InvalidOperationException($"Die Datei {file.FileName} ist zu groß");
        if (FileUploaderViewModel.AllowedContentTypes.All(s => file.ContentType != s))
            throw new InvalidOperationException($"Die Datei {file.FileName} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt");

        var ((files, ids), _, token) = transactionContext;
        
        var name = file.FileName;
        var projectName = new ProjectName(files.JobName);
        
        var id = ProjectFileId.For(projectName, new FileName(name));

        var preRegister = await _contentManager.PreRegisterFile(file.OpenReadStream, id, name, files.JobName, token);

        if (!string.IsNullOrWhiteSpace(preRegister)) throw new InvalidOperationException(preRegister);

        var result = await _fileService.RegisterFile(
            new ProjectFileInfo(
                id,
                projectName,
                new FileName(name),
                new FileSize(file.Length),
                FileType.OtherFile,
                new FileMime(file.ContentType)),
            token);

        if(!string.IsNullOrWhiteSpace(result))
            throw new InvalidOperationException(result);

        ids.Add(id);
        return async _ => await _contentManager.DeleteFile(id, default);
    }
}