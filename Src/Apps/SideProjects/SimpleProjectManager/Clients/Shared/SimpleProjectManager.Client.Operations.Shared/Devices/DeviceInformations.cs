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
        DeviceUi.Empty(),
        ImmutableList<ButtonState>.Empty,
        ActorRefs.Nobody);

    public IEnumerable<DeviceSensor> CollectSensors()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            DeviceUiGroup group = stack.Pop();

            foreach (DeviceUiGroup uiGroup in group.Ui)
                stack.Push(uiGroup);

            switch (group.Type)
            {
                case UIType.SensorDouble:
                    yield return new DeviceSensor(group.Name, group.Id, SensorType.Double);
                    break;
                case UIType.SensorNumber:
                    yield return new DeviceSensor(group.Name, group.Id, SensorType.Double);
                    break;
                case UIType.SensorString:
                    yield return new DeviceSensor(group.Name, group.Id, SensorType.String);
                    break;
            }
        }
    }

    public IEnumerable<(DeviceButton Button, ButtonState? State)> CollectButtons()
    {
        var stack = new Stack<DeviceUiGroup>();
        stack.Push(RootUi);

        while (stack.Count != 0)
        {
            DeviceUiGroup group = stack.Pop();

            foreach (DeviceUiGroup uiGroup in group.Ui)
                stack.Push(uiGroup);

            if(group.Type != UIType.Button) continue;

            var button = new DeviceButton(group.Name, group.Id);
            yield return (button, ButtonStates.FirstOrDefault(s => s.ButtonId == button.Identifer));
        }
    }
}