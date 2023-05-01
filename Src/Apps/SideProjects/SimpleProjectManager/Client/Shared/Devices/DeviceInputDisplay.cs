using System.Reactive.Linq;
using DynamicData.Binding;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.Devices;

public partial class DeviceInputDisplay
{
    [Parameter]
    public DeviceId? DeviceId { get; set; }

    [Parameter]
    public DeviceId? Element { get; set; }

    [Parameter]
    public DisplayName Name { get; set; }

    [Parameter]
    public string Content { get; set; } = string.Empty;

    private MudCommandButton? _sendCommand;
    
    private string InputText
    {
        get => ViewModel?.Input ?? string.Empty;
        set
        {
            if(ViewModel is null) return;
            ViewModel.Input = value;

            OnPropertyChanged();
        } 
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if(ViewModel is null) yield break;

        yield return this.BindCommand(ViewModel, m => m.Send, d => d._sendCommand);
        
        yield return
            (
                from pv in ViewModel.WhenPropertyChanged(m => m.Input)
                where !string.IsNullOrWhiteSpace(pv.Value)
                select pv.Value
            )
            .DistinctUntilChanged()
            .Subscribe(
                s =>
                {
                    InputText = s;
                    RenderingManager.StateHasChangedAsync().Ignore();
                });
    }
}