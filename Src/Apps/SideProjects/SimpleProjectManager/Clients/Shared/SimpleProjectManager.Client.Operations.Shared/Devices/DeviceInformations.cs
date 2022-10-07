namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record DeviceInformations(string DeviceName, bool HasLogs, DeviceUiGroup RootUi)
{
    public const string ManagerName = "DeviceManager";
    
    public const string ManagerPath = "/user/DeviceManager";
}