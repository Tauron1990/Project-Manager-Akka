using System;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

public record struct ErrorData(bool IsOnline, CriticalError[] Errors);

public sealed class CriticalErrorsViewModel : ViewModelBase
{
    public GlobalState GlobalState { get; }
    
    public IObservable<ErrorData> Errors { get; }
    
    public CriticalErrorsViewModel(GlobalState globalState)
    {
        GlobalState = globalState;
        Errors = globalState.IsOnline.CombineLatest(globalState.Errors.Errors, (online, errors) => new ErrorData(online, errors));
    }
}