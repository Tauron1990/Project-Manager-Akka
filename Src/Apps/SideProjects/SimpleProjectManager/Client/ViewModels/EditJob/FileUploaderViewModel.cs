using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Util;
using DynamicData;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileUploaderViewModel : BlazorViewModel
{
    private readonly IEventAggregator _aggregator;
    private readonly SourceCache<FileUploadFile, string> _files = new(bf => bf.Name);
    private readonly BehaviorSubject<bool> _isUploading = new(false);
    private readonly ObservableAsPropertyHelper<bool> _shouldDisable;
    private string? _projectId;

    public bool ShouldDisable => _shouldDisable.Value;
    
    public ReactiveCommand<Unit, Unit> Clear { get; }
    
    public ReactiveCommand<Unit, Unit> Upload { get; }

    public Action<InputFileChangeEventArgs> FilesChanged { get; }

    public ReadOnlyObservableCollection<FileUploadFile> Files { get; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    public FileUploaderViewModel(IStateFactory stateFactory, IEventAggregator aggregator)
        : base(stateFactory)
    {
        _aggregator = aggregator;
        
        _files.DisposeWith(this);

        _files.Connect()
           .AutoRefresh(f => f.UploadState)
           .Bind(out var list)
           .Subscribe()
           .DisposeWith(this);

        Files = list;

        var canExecute = _isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0).Publish().RefCount();

        _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);
        
        Upload = ReactiveCommand.CreateFromObservable(UploadFiles, canExecute)
           .DisposeWith(this);
        Upload.IsExecuting.Subscribe(_isUploading).DisposeWith(this);
        
        Clear = ReactiveCommand.Create(() => _files.Clear(), canExecute)
           .DisposeWith(this);

        var filesChanged = ReactiveCommand.Create<InputFileChangeEventArgs, Unit>(
            args =>
            {
                _files.Edit(u => u.Load(args.GetMultipleFiles().Where(ValidateFile).Select(bf => new FileUploadFile(bf))));
                return Unit.Default;
            })
           .DisposeWith(this);
        FilesChanged = filesChanged.ToAction();

        bool ValidateFile(IBrowserFile file)
        {
            return true;
        }
        
        IObservable<Unit> UploadFiles()
                => Observable.Empty<Unit>();
    }
}