using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.UiHelper;

public abstract class ScreenModelBase : IScreenModel
{
    public DeviceUiGroup Initialize() => throw new NotImplementedException();

    
    
    public void OnShow(UiManagerHelper helper)
    {
        throw new NotImplementedException();
    }

    public void OnHide()
    {
        throw new NotImplementedException();
    }

    public Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor) => throw new NotImplementedException();

    public void ButtonClick(DeviceId identifer)
    {
        throw new NotImplementedException();
    }

    public void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged)
    {
        throw new NotImplementedException();
    }

    public Task<SimpleResult> NewInput(DeviceId element, string input) => throw new NotImplementedException();
}