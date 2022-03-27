using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using SimpleProjectManager.Client.ViewModels;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class FileUploader
{
    private string? _dragEnterStyle;
    
    private MudCommandButton? _clear;
    private MudCommandButton? _upload;

    [Parameter]
    public bool DisableNameEdit { get; set; }

    [Parameter]
    public bool DisableUploadButton { get; set; }

    [Parameter]
    public FileUploadTrigger? UploadTrigger { get; set; }

    [Parameter]
    public FileUploaderViewModel? UploaderViewModel { get; set; }

    [Parameter] 
    public string ProjectName { get; set; } = string.Empty;

    public MudCommandButton? Clear
    {
        get => _clear;
        set => this.RaiseAndSetIfChanged(ref _clear, value);
    }

    public MudCommandButton? Upload
    {
        get => _upload;
        set => this.RaiseAndSetIfChanged(ref _upload, value);
    }

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                this.BindCommand(ViewModel, m => m.Clear, v => v.Clear).DisposeWith(dispo);
                this.BindCommand(ViewModel, m => m.Upload, v => v.Upload).DisposeWith(dispo);
            });
    }

    protected override FileUploaderViewModel CreateModel()
        => UploaderViewModel ?? base.CreateModel();

    private void FilesChanged(InputFileChangeEventArgs evt)
        => ViewModel?.FilesChanged
           .Execute(
                new FileChangeEvent(
                    ImmutableList<IFileReference>.Empty
                       .AddRange(evt.GetMultipleFiles().Select(f => new FileMap(f)))))
           .Catch(Observable.Empty<Unit>())
           .Subscribe();

    private sealed class FileMap : IFileReference
    {
        private readonly IBrowserFile _file;

        public FileMap(IBrowserFile file)
            => _file = file;

        public string Name => _file.Name;
        public string ContentType => _file.ContentType;
        public long Size => _file.Size;
        public Stream OpenReadStream(long maxSize, CancellationToken token)
            => _file.OpenReadStream(maxSize, token);
    }
}