using Akka.Actor;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record DeviceInformations(string DeviceName, bool HasLogs, DeviceUiGroup RootUi, IActorRef DeviceManager)
{
    public const string ManagerName = "DeviceManager";
    
    public const string ManagerPath = "/user/DeviceManager";

    public IEnumerable<DeviceSensor> CollectSensors()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            var group = stack.Pop();

            foreach (var uiGroup in group.Groups)
                stack.Push(uiGroup);

            foreach (var sensor in group.Sensors)
                yield return sensor;
        }
    }
    
    public IEnumerable<DeviceButton> CollectButtons()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            var group = stack.Pop();

            foreach (var uiGroup in group.Groups)
                stack.Push(uiGroup);

            foreach (var button in group.DeviceButtons)
                yield return button;
        }
    }
}