using System.Collections.Immutable;
using Akka.Actor;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record DeviceInformations(DeviceId DeviceId, DeviceName Name, bool HasLogs, DeviceUiGroup RootUi, ImmutableList<ButtonState> ButtonStates, IActorRef DeviceManager)
{
    public const string ManagerName = "DeviceManager";

    public const string ManagerPath = "/user/DeviceManager";
    
    public static DeviceInformations Empty => new(
        DeviceId.New, 
        DeviceName.Empty, 
        HasLogs: false,
        new DeviceUiGroup(
            string.Empty,
            ImmutableList<DeviceUiGroup>.Empty,
            ImmutableList<DeviceSensor>.Empty,
            ImmutableList<DeviceButton>.Empty),
        ImmutableList<ButtonState>.Empty, 
        ActorRefs.Nobody);

    public IEnumerable<DeviceSensor> CollectSensors()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            DeviceUiGroup group = stack.Pop();

            foreach (DeviceUiGroup uiGroup in group.Groups)
                stack.Push(uiGroup);

            foreach (DeviceSensor sensor in group.Sensors)
                yield return sensor;
        }
    }

    public IEnumerable<(DeviceButton Button, ButtonState? State)> CollectButtons()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            DeviceUiGroup group = stack.Pop();

            foreach (DeviceUiGroup uiGroup in group.Groups)
                stack.Push(uiGroup);

            foreach (DeviceButton button in group.DeviceButtons)
                yield return (button, ButtonStates.FirstOrDefault(s => s.ButtonId == button.Identifer));
        }
    }
}