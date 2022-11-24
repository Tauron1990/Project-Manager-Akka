using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public sealed class DeviceViewModel : ViewModelBase
{
    private readonly IMutableState<DevicePair?> _device;

    public DevicePair? DevicePair
    {
        get => _device.Value;
        set => _device.Set(value);
    }

    public IState<DeviceUiGroup?> Ui { get; set; }
    
    public DeviceViewModel(GlobalState state, IStateFactory stateFactory)
    {
        _device = stateFactory.NewMutable<DevicePair?>();

        Ui = state.Devices.GetUiFetcher(
            async t =>
            {
                DevicePair? result = await _device.Use(t).ConfigureAwait(false);

                return result?.Id;
            });
    }
}