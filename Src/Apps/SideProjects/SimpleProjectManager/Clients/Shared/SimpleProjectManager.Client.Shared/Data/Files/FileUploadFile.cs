using System.IO;
using System.Threading;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.States;

namespace SimpleProjectManager.Client.Shared.Data.Files;

public enum UploadState
{
    Pending,
    Uploading,
    Compled
}

public class FileUploadFile : ReactiveObject
{
    private UploadState _uploadState = UploadState.Pending;

    public FileUploadFile(IFileReference uploadFile)
        => UploadFile = uploadFile;

    public IFileReference UploadFile { get; }

    public string Name => UploadFile.Name;

    public string ContentType => UploadFile.ContentType;

    public UploadState UploadState
    {
        get => _uploadState;
        set => this.RaiseAndSetIfChanged(ref _uploadState, value);
    }


    public Stream Open(CancellationToken token)
        => UploadFile.OpenReadStream(FilesState.MaxSize, token);
}