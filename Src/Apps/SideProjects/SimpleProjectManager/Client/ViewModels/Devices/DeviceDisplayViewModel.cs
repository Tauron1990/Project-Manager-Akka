using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Devices;
using SimpleProjectManager.Client.Shared.ViewModels.Devices;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels.Devices;

public sealed class DeviceDisplayViewModel : DeviceViewModel, IParameterUpdateable
{
    public DeviceDisplayViewModel(GlobalState state, IStateFactory stateFactory) : base(state, stateFactory) { }
    
    protected override IState<DevicePair?> CreateDeviceSelector(IStateFactory stateFactory)
        => Updater.Register<DevicePair?>(nameof(DeviceDisplay.ToDisplay), stateFactory);

    public ParameterUpdater Updater { get; } = new();
}