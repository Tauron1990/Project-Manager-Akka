using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Alias;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.EditJob;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileUploaderViewModel : BlazorViewModel
{
    public static readonly string[] AllowedContentTypes = { "application/pdf", "application/x-zip-compressed", "application/zip", "image/tiff", "image/x-tiff" };
    public const long MaxSize = 524_288_000;
    
    private readonly IEventAggregator _aggregator;
    private readonly UploadTransaction _transaction;
    private readonly ICriticalErrorService _criticalErrorService;
    private readonly SourceCache<FileUploadFile, string> _files = new(bf => bf.Name);
    private readonly BehaviorSubject<bool> _isUploading = new(false);
    private readonly ObservableAsPropertyHelper<bool> _shouldDisable;
    private readonly ProjectNameValidator _nameValidator = new();
    private string? _projectId;

    public bool ShouldDisable => _shouldDisable.Value;
    
    public ReactiveCommand<Unit, Unit> Clear { get; }
    
    public ReactiveCommand<Unit, Unit> Upload { get; }

    public Action<InputFileChangeEventArgs> FilesChanged { get; }

    public Func<string, string> ValidateName { get; }

    public ReadOnlyObservableCollection<FileUploadFile> Files { get; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    public FileUploaderViewModel(IStateFactory stateFactory, IEventAggregator aggregator, UploadTransaction transaction, ICriticalErrorService criticalErrorService)
        : base(stateFactory)
    {
        _aggregator = aggregator;
        _transaction = transaction;
        _criticalErrorService = criticalErrorService;

        var nameState = GetParameter<string>(nameof(FileUploader.ProjectName)).ToObservable().StartWith(string.Empty);

        nameState.Subscribe(s => ProjectId = s).DisposeWith(this);
        
        ValidateName = ValidateProjectName;
        
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
        
        Upload = ReactiveCommand.CreateFromTask(UploadFiles, 
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

    private string ValidateProjectName(string? arg)
    {
        var name = new ProjectName(arg ?? string.Empty);
        var result = _nameValidator.Validate(name);
        
        return result.IsValid ? string.Empty : string.Join(", ", result.Errors.Select(err => err.ErrorMessage));
    }

    private bool IsFileValid(IBrowserFile file)
    {
        var result = AllowedContentTypes.Any(t => t == file.ContentType);
        if(!result)
            _aggregator.PublishWarnig($"Die Datei {file.Name} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt");

        return result;
    }

    private async Task UploadFiles()
    {
        var validation = ValidateProjectName(ProjectId);
        if (!string.IsNullOrWhiteSpace(validation))
        {
            _aggregator.PublishWarnig(validation);
            return;
        }
        if(string.IsNullOrWhiteSpace(ProjectId)) return;
        
        var context = new UploadTransactionContext(Files.ToImmutableList(), new ProjectName(ProjectId));

        var (trasnactionState, exception) = await _transaction.Execute(context);
        switch (trasnactionState)
        {
            case TrasnactionState.Successeded:
                return;
            case TrasnactionState.Rollback when exception is not null:
                try
                {
                    var newEx = exception.Demystify();

                    await _criticalErrorService.WriteError(
                        new CriticalError(
                            string.Empty,
                            DateTime.Now,
                            $"{nameof(FileUploaderViewModel)} -- {nameof(UploadFiles)}",
                            newEx.Message,
                            newEx.StackTrace,
                            ImmutableList<ErrorProperty>.Empty),
                        default);
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(exception);
                    _aggregator.PublishError(e);
                }
                break;
            case TrasnactionState.RollbackFailed when exception is not null:
                _aggregator.PublishError(exception);
                break;
            default:
                _aggregator.PublishWarnig("Datei Upload Unbekannter zustand");
                break;
        }
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