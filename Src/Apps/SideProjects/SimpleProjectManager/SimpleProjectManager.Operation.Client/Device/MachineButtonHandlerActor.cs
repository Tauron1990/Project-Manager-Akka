using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;

using static SimpleProjectManager.Client.Operations.Shared.Devices.DeviceManagerMessages;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class MachineButtonHandlerActor : ReceiveActor
{
    private readonly string _deviceName;
    private readonly IMachine _machine;
    private readonly DeviceButton _button;
    private readonly IActorRef _deviceDeviceManager;

    private bool _state;
    
    public MachineButtonHandlerActor(string deviceName, IMachine machine, DeviceButton button, IActorRef deviceDeviceManager)
    {
        _deviceName = deviceName;
        _machine = machine;
        _button = button;
        _deviceDeviceManager = deviceDeviceManager;

        Receive<ButtonClick>(OnButtonClick);
        _machine.WhenButtonStateChanged(_button.Identifer, OnButtonStateChanged);
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