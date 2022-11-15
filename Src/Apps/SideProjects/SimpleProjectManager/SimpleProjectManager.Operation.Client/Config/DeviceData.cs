using SimpleProjectManager.Operation.Client.Device;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed record DeviceData(bool Active, InterfaceId MachineInterface)
{
    public DeviceData()
        : this(Active: false, InterfaceId.Empty)
    {}
}