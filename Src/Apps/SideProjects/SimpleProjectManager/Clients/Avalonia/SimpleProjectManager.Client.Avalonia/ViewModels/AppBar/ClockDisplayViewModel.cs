using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.ViewModels;
using Stl.Fusion;
using Stl.Fusion.Extensions;
using Tauron;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

public sealed class ClockDisplayViewModel : ViewModelBase
{
    private ObservableAsPropertyHelper<string>? _currentTime;

    public ClockDisplayViewModel(IFusionTime time, IStateFactory stateFactory)
    {
        this.WhenActivated(Init);

        IEnumerable<IDisposable> Init()
        {
            var state = stateFactory.NewComputed<DateTime>(ComputeState);

            yield return state;

            yield return _currentTime = state
               .ToObservable(_ => true)
               .Select(dt => dt.ToLocalTime().ToString("G"))
               .ObserveOn(RxApp.MainThreadScheduler)
               .ToProperty(this, model => model.CurrentTime);
        }

        async Task<DateTime> ComputeState(IComputedState<DateTime> _, CancellationToken cancellationToken) => await time.GetUtcNow(TimeSpan.FromMilliseconds(200));
    }

    public string CurrentTime => _currentTime?.Value ?? string.Empty;
}