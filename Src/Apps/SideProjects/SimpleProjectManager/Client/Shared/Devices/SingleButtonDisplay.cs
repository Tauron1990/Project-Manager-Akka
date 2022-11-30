using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class SingleButtonDisplay
{
    private MudCommandButton? _commandButton;

    [Parameter]
    public DeviceId? DeviceId { get; set; }

    [Parameter]
    public DeviceButton? Button { get; set; }


    private MudCommandButton? CommandButton
    {
        get => _commandButton;
        set => this.RaiseAndSetIfChanged(ref _commandButton, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.ButtonClick, m => m.CommandButton);
    }
}