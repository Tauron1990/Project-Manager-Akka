using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using Tauron.Application.Blazor;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Pages;

public partial class CurrentJobs
{
    private CompositeDisposable _compositeDisposable = new();
    private MudCommandButton? _newJob;

    private MudCommandButton? NewJob
    
    {
        get => _newJob;
        set => this.RaiseAndSetIfChanged(ref _newJob, value);
    }

    [Parameter]
    public string? PreSelected { get; set; }
    
    protected override IEnumerable<IDisposable> InitializeModel()
    {
        _compositeDisposable = new CompositeDisposable();

        yield return _compositeDisposable;
        yield return this.BindCommand(ViewModel, m => m.NewJob, v => v.NewJob);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Task.Run(
            async () =>
                 {
                     if(string.IsNullOrWhiteSpace(PreSelected)) return Task.CompletedTask;

                     await GlobalState.Jobs.IsLoaded.Where(l => l).Take(1).FirstOrDefaultAsync();
                     
                     GlobalState.Dispatch(new SelectNewPairAction(PreSelected));

                     return Task.CompletedTask;
                 });
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if(firstRender)
        {
            GlobalState.Jobs.CurrentlySelectedPair
               .ObserveOn(RxApp.MainThreadScheduler)
               .SelectMany(
                    async d =>
                    {
                        try
                        {
                            if(d?.Order is null)
                                return Unit.Default;

                            await JsRuntime.InvokeVoidAsync("window.applyUrl", $"/CurrentJobs/{d.Order.Id.Value}");

                            return Unit.Default;
                        }
                        catch (Exception exception)
                        {
                            EventAggregator.PublishError(exception);

                            return Unit.Default;
                        }
                    })
               .Subscribe()
               .DisposeWith(_compositeDisposable);
        }
        base.OnAfterRender(firstRender);
    }
}