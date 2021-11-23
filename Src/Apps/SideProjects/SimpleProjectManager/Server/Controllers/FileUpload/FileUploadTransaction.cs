using JetBrains.Annotations;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application;

namespace SimpleProjectManager.Server.Controllers.FileUpload;

public sealed record FileUploadContext(UploadFiles Files, FileContentManager FileContentManager, IJobFileService FileService, CancellationToken Token);

public class FileUploadTransaction : SimpleTransaction<FileUploadContext>
{
    public FileUploadTransaction()
        => RegisterLoop(c => c.Files.Files, UploadTempFile);

    private static async ValueTask<Rollback<FileUploadContext>> UploadTempFile(FileUploadContext context, IFormFile file)
    {
        if(file.Length > FileUploaderViewModel.MaxSize)
            throw new InvalidOperationException($"Die Datei {file.FileName} ist zu groß");
        if (FileUploaderViewModel.AllowedContentTypes.All(s => file.ContentType != s))
            throw new InvalidOperationException($"Die Datei {file.FileName} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt");
            
        var name = file.FileName;
        var projectName = new ProjectName(context.Files.JobName);

        var (_, contentManager, service, cancellationToken) = context;
        
        var id = ProjectFileId.For(projectName, new FileName(name));

        var preRegister = await contentManager.PreRegisterFile(file.OpenReadStream, id, name, cancellationToken);

        if (!string.IsNullOrWhiteSpace(preRegister)) throw new InvalidOperationException(preRegister);

        var result = await service.RegisterFile(
            new ProjectFileInfo(
                id,
                projectName,
                new FileName(name),
                new FileSize(file.Length),
                FileType.OtherFile,
                new FileMime(file.ContentType)),
            cancellationToken);

        if(!string.IsNullOrWhiteSpace(result))
            throw new InvalidOperationException(result);

        return async _ => await contentManager.DeleteFile(id, default);
    }
}