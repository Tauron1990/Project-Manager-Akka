using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

public abstract class CriticalErrorViewModelBase : ViewModelBase
{
    private ObservableAsPropertyHelper<CriticalError?>? _item;
    public CriticalError? Item => _item?.Value;

    public ReactiveCommand<Unit, Unit>? Hide { get; private set; }

    protected CriticalErrorViewModelBase(GlobalState globalState)
    {
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            var currentError = GetErrorState();
            
            yield return _item = currentError.ToObservable().ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, m => m.Item);
            yield return Hide = ReactiveCommand.Create(
                () =>
                {
                    var err = currentError.ValueOrDefault;

                    if (err is null) return;

                    globalState.Dispatch(new DisableError(err));
                },
                currentError.ToObservable().Select(d => d is not null).StartWith(false)
                   .AndIsOnline(globalState.OnlineMonitor));
        }
    }

    protected abstract IState<CriticalError?> GetErrorState();
}