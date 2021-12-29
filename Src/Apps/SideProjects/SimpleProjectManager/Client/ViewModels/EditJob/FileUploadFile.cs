using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;

namespace SimpleProjectManager.Client.ViewModels;

public enum UploadState
{
    Pending,
    Uploading,
    Compled
}

public class FileUploadFile : ReactiveObject
{
    private UploadState _uploadState = UploadState.Pending;
    public IBrowserFile BrowserFile { get; }

    public string Name => BrowserFile.Name;

    public string ContentType => BrowserFile.ContentType;

    public UploadState UploadState
    {
        get => _uploadState;
        set => this.RaiseAndSetIfChanged(ref _uploadState, value);
    }


    public Stream Open(CancellationToken token)
        => BrowserFile.OpenReadStream(FileUploaderViewModel.MaxSize, token);

    public FileUploadFile(IBrowserFile browserFile)
        => BrowserFile = browserFile;
}