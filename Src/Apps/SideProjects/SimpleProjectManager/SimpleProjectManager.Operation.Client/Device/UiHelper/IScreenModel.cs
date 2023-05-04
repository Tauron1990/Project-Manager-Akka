using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device.UiHelper;

public interface IScreenModel
{
    DeviceUiGroup Initialize();
    
    void OnShow();

    void OnHide();
    
    Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor);
    void ButtonClick(DeviceId identifer);
    void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged);

    Task<SimpleResult> NewInput(DeviceId element, string input);
}