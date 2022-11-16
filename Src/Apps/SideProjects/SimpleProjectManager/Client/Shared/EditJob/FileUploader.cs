using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class FileUploader
{
    private MudCommandButton? _clear;
    private string? _dragEnterStyle;
    private MudCommandButton? _upload;

    [Parameter]
    public bool DisableNameEdit { get; set; }

    [Parameter]
    public bool DisableUploadButton { get; set; }

    [Parameter]
    public FileUploadTrigger? UploadTrigger { get; set; }

    [Parameter]
    public FileUploaderViewModelBase? UploaderViewModel { get; set; }

    [Parameter]
    public string ProjectName { get; set; } = string.Empty;

    [PublicAPI]
    public MudCommandButton? Clear
    {
        get => _clear;
        private set => this.RaiseAndSetIfChanged(ref _clear, value);
    }

    [PublicAPI]
    public MudCommandButton? Upload
    {
        get => _upload;
        private set => this.RaiseAndSetIfChanged(ref _upload, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.Clear, v => v.Clear);
        yield return this.BindCommand(ViewModel, m => m.Upload, v => v.Upload);
    }

    protected override FileUploaderViewModel CreateModel()
        => UploaderViewModel as FileUploaderViewModel ?? base.CreateModel();

    private void FilesChanged(InputFileChangeEventArgs evt)
        => ViewModel?.FilesChanged?
           .Execute(
                new FileChangeEvent(
                    ImmutableList<IFileReference>.Empty
                       .AddRange(evt.GetMultipleFiles().Select(f => new FileMap(f)))))
           .Catch(Observable.Empty<Unit>())
           .Subscribe();

    private sealed class FileMap : IFileReference
    {
        private readonly IBrowserFile _file;

        internal FileMap(IBrowserFile file)
        {
            _file = file;
            Size = new FileSize(file.Size);
            Name = new FileName(file.Name);
            ContentType = new FileMime(file.ContentType);
        }

        public FileName Name { get; }
        public FileMime ContentType { get; }
        public FileSize Size { get; }

        public Stream OpenReadStream(in MaxSize maxSize, CancellationToken token)
            => _file.OpenReadStream(maxSize.Value, token);
    }
}