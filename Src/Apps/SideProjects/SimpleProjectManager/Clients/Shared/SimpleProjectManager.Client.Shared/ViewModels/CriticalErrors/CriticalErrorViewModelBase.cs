using System.Reactive;
using System.Reactive.Disposables;
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

    public ReactiveCommand<Unit, Unit> Hide { get; }

    protected CriticalErrorViewModelBase(GlobalState globalState)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        var currentError = GetErrorState();
        
        Hide = ReactiveCommand.Create(
                () =>
                {
                    var err = currentError.ValueOrDefault;

                    if (err is null) return;

                    globalState.Dispatch(new DisableError(err));
                },
                currentError.ToObservable().Select(d => d is not null).StartWith(false)
                   .AndIsOnline(globalState.OnlineMonitor))
           .DisposeWith(Disposer);
        
        this.WhenActivated(dispo => _item = currentError.ToObservable().ToProperty(this, m => m.Item).DisposeWith(dispo));
    }

    protected abstract IState<CriticalError?> GetErrorState();
}