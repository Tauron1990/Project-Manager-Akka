using Akkatecture.Core;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed class DeviceId : Identity<DeviceId>
{
    public DeviceId(string value) : base(value) { }
}