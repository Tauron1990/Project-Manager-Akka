using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Alias;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileUploaderViewModel : BlazorViewModel
{
    private static readonly string[] AllowedContentTypes = { "application/pdf", "application/x-zip-compressed", "application/zip", "image/tiff", "image/x-tiff" };
    
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
        
        _files.Connect()
           .Where(f => f.UploadState == UploadState.Pending && string.IsNullOrWhiteSpace(ProjectId))
           .Select(TryExtrectName)
           .Flatten()
           .Where(n => !string.IsNullOrWhiteSpace(n.Current) && string.IsNullOrWhiteSpace(ProjectId))  
           .Do(n => ProjectId = n.Current)
           .Subscribe()
           .DisposeWith(this);
        
        Files = list;

        var canExecute = _isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0).Publish().RefCount();

        _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);
        
        Upload = ReactiveCommand.CreateFromObservable(UploadFiles, 
                canExecute.CombineLatest(this.WhenAnyValue(m => m.ProjectId)).Select(t => t.First && !string.IsNullOrWhiteSpace(t.Second)))
           .DisposeWith(this);
        Upload.IsExecuting.Subscribe(_isUploading).DisposeWith(this);
        
        Clear = ReactiveCommand.Create(() => _files.Clear(), canExecute)
           .DisposeWith(this);

        var filesChanged = ReactiveCommand.Create<InputFileChangeEventArgs, Unit>(
            args =>
            {
                _files.Edit(u => u.Load(args.GetMultipleFiles()
                               .Where(IsFileValid)
                               .Select(bf => new FileUploadFile(bf))));
                return Unit.Default;
            })
           .DisposeWith(this);
        FilesChanged = filesChanged.ToAction();
    }

    private bool IsFileValid(IBrowserFile file)
    {
        var result = AllowedContentTypes.Any(t => t == file.ContentType);
        if(!result)
            _aggregator.PublishWarnig($"Die Datei {file.Name} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt");

        return result;
    }

    IObservable<Unit> UploadFiles()
    {
        //TODO Implement File Upload
        return Observable.Empty<Unit>();
    }

    private static string? TryExtrectName(FileUploadFile file)
    {
        var upperName = file.Name.ToUpper().AsSpan();

        var index = upperName.IndexOf("BM");

        if (index == -1) return null;

        while (upperName.Length >= 10)
        {
            upperName = upperName[index..];
            if(upperName.Length != 10) break;

            if (AllDigit(upperName[2..2]) &&
                upperName[4] == '_' &&
                AllDigit(upperName[5..10]))
            {
                return upperName[..10].ToString();
            }
        }

        return null;
    }

    private static bool AllDigit(in ReadOnlySpan<char> input)
    {
        foreach (var t in input)
        {
            if(char.IsDigit(t)) continue;

            return false;
        }

        return true;
    }
}