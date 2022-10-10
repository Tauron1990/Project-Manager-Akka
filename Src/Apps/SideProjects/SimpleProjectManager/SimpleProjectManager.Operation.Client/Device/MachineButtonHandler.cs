using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device;

public sealed class MachineButtonHandler : ReceiveActor
{
    private readonly IMachine _machine;
    private readonly DeviceButton _button;

    public MachineButtonHandler(IMachine machine, DeviceButton button, IActorRef deviceDeviceManager)
    {
        _machine = machine;
        _button = button;

    }
}