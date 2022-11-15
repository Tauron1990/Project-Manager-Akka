using Akka.Actor;
using SimpleProjectManager.Shared.Services.Devices;
using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class MachineButtonHandlerActor : ReceiveActor
{
    private readonly DeviceButton _button;
    private readonly IActorRef _deviceDeviceManager;
    private readonly DeviceId _deviceName;
    private readonly IMachine _machine;

    private bool _state;

    public MachineButtonHandlerActor(DeviceId deviceName, IMachine machine, DeviceButton button, IActorRef deviceDeviceManager)
    {
        _deviceName = deviceName;
        _machine = machine;
        _button = button;
        _deviceDeviceManager = deviceDeviceManager;

        Receive<ButtonClick>(OnButtonClick);
        machine.WhenButtonStateChanged(button.Identifer, OnButtonStateChanged);
    }

    private void OnButtonStateChanged(bool state)
    {
        if(state == _state) return;

        _state = state;

        _deviceDeviceManager.Tell(new UpdateButtonState(_deviceName, _button.Identifer, _state));
    }

    private void OnButtonClick(ButtonClick obj)
        => _machine.ButtonClick(obj.Identifer);
}