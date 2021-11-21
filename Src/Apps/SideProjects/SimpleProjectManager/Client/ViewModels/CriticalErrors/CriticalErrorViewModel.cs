using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.CriticalErrors;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public class CriticalErrorViewModel : BlazorViewModel
{
    private readonly ObservableAsPropertyHelper<CriticalError?> _item;
    public CriticalError? Item => _item.Value;

    public ReactiveCommand<Unit, Unit> Hide { get; }

    public CriticalErrorViewModel(IStateFactory stateFactory, ICriticalErrorService errorService, IEventAggregator eventAggregator)
        : base(stateFactory)
    {
        var currentError = GetParameter<CriticalError?>(nameof(CriticalErrorDispaly.Error));
        _item = currentError.ToObservable().ToProperty(this, m => m.Item).DisposeWith(this);
        
        Hide = ReactiveCommand.CreateFromTask(
            async () =>
            {
                var err = currentError.ValueOrDefault;
                if(err is null) return;
                await eventAggregator.IsSuccess(() => TimeoutToken.WithDefault(
                                                    t => errorService.DisableError(err.Id, t)));
            }, currentError.ToObservable().Select(d => d is not null).StartWith(false))
           .DisposeWith(this);
    }
}