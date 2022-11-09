using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.ViewModels;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

public sealed class NotifyErrorModel : ViewModelBase
{
    private ObservableAsPropertyHelper<string>? _errorCount;

    private ObservableAsPropertyHelper<bool>? _hasErrors;

    public NotifyErrorModel(GlobalState state, PageNavigation pageNavigation, IOnlineMonitor monitor)
    {
        this.WhenActivated(CreateObject);

        IEnumerable<IDisposable> CreateObject()
        {
            var errors = state.Errors.ErrorCount
               .CombineLatest(monitor.Online, (errorCount, isOnline) => (isOnline, errorCount))
               .Publish().RefCount();

            yield return NavigateError = ReactiveCommand.Create(pageNavigation.Errors);

            yield return _errorCount = errors
               .Select(p => p.isOnline ? p.errorCount.ToString() : "Keine Verbindung")
               .ObserveOn(RxApp.MainThreadScheduler)
               .ToProperty(this, m => m.ErrorCount);

            yield return _hasErrors = errors
               .Select(e => !e.isOnline || e.errorCount > 0)
               .ObserveOn(RxApp.MainThreadScheduler)
               .ToProperty(this, m => m.HasErrors);
        }
    }

    public ReactiveCommand<Unit, Unit>? NavigateError { get; private set; }
    public string ErrorCount => _errorCount?.Value ?? string.Empty;
    public bool HasErrors => _hasErrors?.Value ?? false;
}