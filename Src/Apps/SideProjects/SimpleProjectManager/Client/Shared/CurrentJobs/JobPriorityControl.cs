using System.Collections.Immutable;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobPriorityControl
{
    private MudCommandButton? _priorize;
    private MudCommandButton? _goup;
    private MudCommandButton? _godown;

    private MudCommandButton? Priorize
    {
        get => _priorize;
        set => this.RaiseAndSetIfChanged(ref _priorize, value);
    }

    private MudCommandButton? Goup
    {
        get => _goup;
        set => this.RaiseAndSetIfChanged(ref _goup, value);
    }

    private MudCommandButton? Godown
    {
        get => _godown;
        set => this.RaiseAndSetIfChanged(ref _godown, value);
    }

    [Parameter]
    public ImmutableList<JobSortOrderPair> ActivePairs { get; set; } = ImmutableList<JobSortOrderPair>.Empty;

    protected override JobPriorityViewModel CreateModel()
        => Services.GetRequiredService<JobPriorityViewModel>();

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                this.BindCommand(ViewModel, m => m.GoDown, v => v.Godown, _jobsViewModel.CurrentInfo).DisposeWith(dispo);
                this.BindCommand(ViewModel, m => m.GoUp, v => v.Goup, _jobsViewModel.CurrentInfo).DisposeWith(dispo);
                this.BindCommand(ViewModel, m => m.Priorize, v => v.Priorize, _jobsViewModel.CurrentInfo).DisposeWith(dispo);
            });
    }
}