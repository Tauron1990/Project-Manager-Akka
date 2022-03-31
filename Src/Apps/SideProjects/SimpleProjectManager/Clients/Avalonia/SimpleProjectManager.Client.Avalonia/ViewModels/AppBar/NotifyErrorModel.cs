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
    public ReactiveCommand<Unit, Unit>? NavigateError { get; set; }

    private ObservableAsPropertyHelper<long>? _errorCount;
    public long ErrorCount => _errorCount?.Value ?? 0;

    private ObservableAsPropertyHelper<bool>? _hasErrors;
    public bool HasErrors => _hasErrors?.Value ?? false;

    public NotifyErrorModel(GlobalState state, PageNavigation pageNavigation)
    {
        this.WhenActivated(CreateObject);
        
        IEnumerable<IDisposable> CreateObject()
        {
            var errors = state.Errors.ErrorCount.Publish().RefCount();
            
            yield return NavigateError = ReactiveCommand.Create(pageNavigation.Errors);
            yield return _errorCount = errors.ToProperty(this, m => m.ErrorCount);
            yield return _hasErrors = errors.Select(e => e > 0).ToProperty(this, m => m.HasErrors);
        }
    }
}