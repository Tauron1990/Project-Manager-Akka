using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Shared.Services.Devices;
using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Operation.Client.Device;

public interface IMachine
{
    Task Init(IActorContext context);

    Task<DeviceInformations> CollectInfo();

    IState<DeviceUiGroup>? UIUpdates();

    Task<DeviceManagerMessages.ISensorBox> UpdateSensorValue(DeviceSensor sensor);
    void ButtonClick(DeviceId identifer);
    void WhenButtonStateChanged(DeviceId identifer, Action<bool> onButtonStateChanged);

    Task<SimpleResult> NewInput(DeviceId element, string input);

    Task<LogBatch> NextLogBatch();
}