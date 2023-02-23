using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using RestEase;
using SimpleProjectManager.Client.Shared.LogFiles;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services.LogFiles;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public  sealed class LogFileDisplayViewModel : BlazorViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogsServiceDef _service;

    private readonly SourceCache<LogData, string> _entries;
    private readonly ObservableAsPropertyHelper<bool> _hasFile;
    
    public bool HasFile => _hasFile.Value;

    public ReadOnlyObservableCollection<LogData> Logs { get; }
    
    public LogFileDisplayViewModel(IStateFactory stateFactory, IEventAggregator eventAggregator)
        : base(stateFactory)
    {
        _eventAggregator = eventAggregator;
        _service = RestClient.For<ILogsServiceDef>();
        
        var input = GetParameter<TargetFileSelection>(nameof(LogFileDisplay.ToDisplay))
            .ToObservable(eventAggregator.PublishError);

        _entries = new SourceCache<LogData, string>(d => d.Date).DisposeWith(this);
        input
            .SelectMany(FetchData)
            .Subscribe(UpdateLogData)
            .DisposeWith(this);
        
        _hasFile = input
            .Select(s => s?.IsT1 ?? false)
            .ToProperty(this, m => m.HasFile)
            .DisposeWith(this);

        _entries.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var logList)
            .Subscribe()
            .DisposeWith(this);

        Logs = logList;
    }

    private void UpdateLogData(LogFileContent content)
    {
        _entries.Edit(
            u =>
            {
                u.Clear();
                if(content == LogFileContent.Empty)
                    return;

                try
                {
                    foreach (LogData logData in LogDataParser.ParseLogs(content.Content))
                    {
                        u.AddOrUpdate(logData);
                    }
                }
                catch (Exception e)
                {
                    _eventAggregator.PublishError(e);
                }
            });
    }

    private CancellationTokenSource? _onGoing;

    private async Task<LogFileContent> FetchData(TargetFileSelection? selection)
    {
        if(selection is null)
            return LogFileContent.Empty;

        try
        {
            _onGoing?.Cancel();
            using var onGoing = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            _onGoing = onGoing;

            return await selection.Match(
                    tf => _service.GetLogFileContent(new LogFileRequest(tf.Host, tf.Name), onGoing.Token),
                    _ => Task.FromResult(LogFileContent.Empty))
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
            return LogFileContent.Empty;
        }
        finally
        {
            _onGoing = null;
        }
    }
}