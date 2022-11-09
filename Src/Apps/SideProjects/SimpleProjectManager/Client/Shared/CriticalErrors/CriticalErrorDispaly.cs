using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Shared.Services;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CriticalErrors;

public partial class CriticalErrorDispaly
{
    //private bool _firstExpanded = true;
    private MudCommandButton? _hide;

    [Parameter]

    public CriticalError? Error { get; set; }

    public MudCommandButton? Hide
    {
        get => _hide;
        set => this.RaiseAndSetIfChanged(ref _hide, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.WhenActivated(
            dispo => { this.BindCommand(ViewModel, m => m.Hide, v => v.Hide).DisposeWith(dispo); });
    }
}