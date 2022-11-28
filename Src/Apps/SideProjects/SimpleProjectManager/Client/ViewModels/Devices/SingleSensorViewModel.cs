using ReactiveUI;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public sealed class SingleSensorViewModel : BlazorViewModel
{
    public SingleSensorViewModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        
    }
}