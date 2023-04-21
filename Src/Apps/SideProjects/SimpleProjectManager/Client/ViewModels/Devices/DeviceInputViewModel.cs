using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public class DeviceInputViewModel : BlazorViewModel
{
    public string Input { get; set; }
    
    protected DeviceInputViewModel(IStateFactory stateFactory) : base(stateFactory)
    {
    }
}