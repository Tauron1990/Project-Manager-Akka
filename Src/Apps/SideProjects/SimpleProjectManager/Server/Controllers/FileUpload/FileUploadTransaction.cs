using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Controllers.FileUpload;

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

        #pragma warning disable GU0017
        ((UploadFiles files, var ids), _, CancellationToken token) = transactionContext;
        #pragma warning restore GU0017

        string name = file.FileName;
        var projectName = new ProjectName(files.JobName);

        ProjectFileId id = ProjectFileId.For(projectName, new FileName(name));

        SimpleResult preRegister = await _contentManager.PreRegisterFile(file.OpenReadStream, id, name, files.JobName, token).ConfigureAwait(false);

        if(preRegister.IsError()) throw new InvalidOperationException(preRegister.GetErrorString());

        SimpleResult result = await _fileService.RegisterFile(
            new ProjectFileInfo(
                id,
                projectName,
                new FileName(name),
                new FileSize(file.Length),
                FileType.OtherFile,
                new FileMime(file.ContentType)),
            token).ConfigureAwait(false);

        if(result.IsError())
            throw result.GetException();

        ids.Add(id);

        return async _ => await _contentManager.DeleteFile(id, default).ConfigureAwait(false);
    }
}