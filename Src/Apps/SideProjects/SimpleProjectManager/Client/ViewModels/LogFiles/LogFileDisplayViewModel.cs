using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.LogFiles;
using SimpleProjectManager.Client.Shared.ViewModels;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.LogFiles;

public  sealed class LogFileDisplayViewModel : BlazorViewModel
{
    private readonly SourceCache<LogData, string> _entries;
    private readonly ObservableAsPropertyHelper<bool> _hasFile;
    public bool HasFile => _hasFile.Value;

    
    public LogFileDisplayViewModel(IStateFactory stateFactory, IEventAggregator eventAggregator)
        : base(stateFactory)
    {
        var input = GetParameter<TargetFileSelection>(nameof(LogFileDisplay.ToDisplay))
            .ToObservable(eventAggregator.PublishError);

        _entries = new SourceCache<LogData, string>(d => d.Date).DisposeWith(this);
        
        _hasFile = input
            .Select(s => s?.IsT1 ?? false)
            .ToProperty(this, m => m.HasFile)
            .DisposeWith(this);
    }
}