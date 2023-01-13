using SimpleProjectManager.Server.Core.DeviceManager.Events;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed class DeviceModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient(sp => sp.GetRequiredService<IEventAggregator>().GetEvent<DeviceEventHandler, IDeviceEvent>());
    }
}