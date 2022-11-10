using System.IO;
using System.Threading;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared;

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

    public FileName Name => UploadFile.Name;

    public FileMime ContentType => UploadFile.ContentType;

    public UploadState UploadState
    {
        get => _uploadState;
        set => this.RaiseAndSetIfChanged(ref _uploadState, value);
    }


    public Stream Open(CancellationToken token)
        => UploadFile.OpenReadStream(MaxSize.Max, token);
}