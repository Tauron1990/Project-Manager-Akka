using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Alias;
using ReactiveUI;
using RestEase;
using SimpleProjectManager.Client.Shared.ViewModels;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services.LogFiles;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public sealed class LogFilesViewModel : ViewModelBase, IDisposable
{
    private readonly SourceCache<LogFileEntry, string> _logFiles = new(e => e.HostName);
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogsServiceDef _logsService;
    private readonly CompositeDisposable _subscriptions;
    
    private string _hostToShow = string.Empty;
    private string _file = string.Empty;

    public string HostToShow
    {
        get => _hostToShow;
        set => this.RaiseAndSetIfChanged(ref _hostToShow, value);
    }

    public string File
    {
        get => _file;
        set => this.RaiseAndSetIfChanged(ref _file, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshLogs { get; }
    
    public ReadOnlyObservableCollection<string> Hosts { get; }
    public ReadOnlyObservableCollection<string> Files { get; }

    public IObservable<TargetFileSelection> CurrentFile { get; }
    
    public LogFilesViewModel(HttpClient httpClient, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _logsService = RestClient.For<ILogsServiceDef>(httpClient);
        
        RefreshLogs = ReactiveCommand.CreateFromTask(UpdateLogData);

        _subscriptions = new CompositeDisposable();
        
        _subscriptions.Add(_logFiles.Connect()
            .Select(e => e.HostName)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var hosts)
            .Subscribe());
        
        _subscriptions.Add(_logFiles.Connect()
            .InvalidateWhen(this.WhenAnyValue(m => m.HostToShow))
            .Where(e => string.Equals(e.HostName, HostToShow, StringComparison.Ordinal))
            .TransformMany(e => e.Files.AsEnumerable(), s => s)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var files)
            .Subscribe());

        Hosts = hosts;
        Files = files;

        CurrentFile = this.WhenAnyValue(m => HostToShow)
            .CombineLatest(this.WhenAnyValue(m => m.File))
            .Select(
                d => string.IsNullOrWhiteSpace(d.First) || string.IsNullOrWhiteSpace(d.Second)
                    ? TargetFileSelection.NoFile
                    : new TargetFile(d.First, d.Second))
            .StartWith(TargetFileSelection.NoFile)
            .Publish().RefCount();
        
        
        this.WhenActivated((CompositeDisposable _) => RefreshLogs.Execute().Subscribe());
    }

    private async Task UpdateLogData()
    {
        try
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            
            _logFiles.Clear();
            LogFilesData result = await _logsService.GetFileNames(tokenSource.Token).ConfigureAwait(false);
            
            _logFiles.Edit(su => result.Entries.ForEach(su.AddOrUpdate));
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }

    public void Dispose()
    {
        _logFiles.Dispose();
        _subscriptions.Dispose();
        RefreshLogs.Dispose();
    }
}