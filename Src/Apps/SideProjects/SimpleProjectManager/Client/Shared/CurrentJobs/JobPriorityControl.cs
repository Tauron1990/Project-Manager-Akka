

using System.Collections.Immutable;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data.States;
using Tauron;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobPriorityControl
{
    private MudCommandButton? _priorize;
    private MudCommandIconButton? _goup;
    private MudCommandIconButton? _godown;

    private MudCommandButton? Priorize
    {
        get => _priorize;
        set => this.RaiseAndSetIfChanged(ref _priorize, value);
    }

    private MudCommandIconButton? Goup
    {
        get => _goup;
        set => this.RaiseAndSetIfChanged(ref _goup, value);
    }

    private MudCommandIconButton? Godown
    {
        get => _godown;
        set => this.RaiseAndSetIfChanged(ref _godown, value);
    }

    [Parameter]
    public ImmutableList<JobSortOrderPair> ActivePairs { get; set; } = ImmutableList<JobSortOrderPair>.Empty;

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        var currentInfo = _globalState.Jobs.CurrentlySelectedPair.NotNull();

        yield return this.BindCommand(ViewModel, m => m.GoDown, v => v.Godown, currentInfo);
        yield return this.BindCommand(ViewModel, m => m.GoUp, v => v.Goup, currentInfo);
        yield return this.BindCommand(ViewModel, m => m.Priorize, v => v.Priorize, currentInfo);
    }
}