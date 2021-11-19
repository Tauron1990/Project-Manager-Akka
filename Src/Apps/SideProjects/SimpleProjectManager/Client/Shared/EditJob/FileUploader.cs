using System.Reactive.Disposables;
using ReactiveUI;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class FileUploader
{
    private string? _dragEnterStyle;
    
    private MudCommandButton? _clear;
    private MudCommandButton? _upload;

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
}