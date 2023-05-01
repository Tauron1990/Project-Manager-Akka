using System.Collections.Immutable;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared.Services.Devices;

[PublicAPI]
public static class DeviceUi
{
    public static DeviceButton ToButton(DeviceUiGroup group) => new(group.Name, group.Id);

    public static DeviceSensor ToSensor(DeviceUiGroup group)
        => new(
            group.Name,
            group.Id,
            group.Type switch
            {
                UIType.SensorString => SensorType.String,
                UIType.SensorDouble => SensorType.Double,
                UIType.SensorNumber => SensorType.Number,
                _ => throw new InvalidOperationException("UI is no Sensor"),
            });
    
    public static DeviceUiGroup Tab(in DisplayName name, IEnumerable<DeviceUiGroup> tabs)
        => new(name, UIType.Tab, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty.AddRange(tabs));
    
    public static DeviceUiGroup Tab(in DisplayName name, params DeviceUiGroup[] tabs)
        => Tab(name, tabs.AsEnumerable());
    
    public static DeviceUiGroup Tab(string name, params DeviceUiGroup[] tabs)
        => Tab(DisplayName.From(name), tabs);
    
    public static DeviceUiGroup Tab(string name, IEnumerable<DeviceUiGroup> tabs)
        => Tab(DisplayName.From(name), tabs);
    
    public static DeviceUiGroup Group(in DisplayName name, params DeviceUiGroup[] content) 
        => Group(name, content.AsEnumerable());

    public static DeviceUiGroup Group(string name, params DeviceUiGroup[] content)
        => Group(DisplayName.From(name), content);

    public static DeviceUiGroup Group(string name, IEnumerable<DeviceUiGroup> content) 
        => Group(DisplayName.From(name), content);
    
    public static DeviceUiGroup Group(in DisplayName name, IEnumerable<DeviceUiGroup> content) 
        => new(name, UIType.Group, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty.AddRange(content));
    
    public static DeviceUiGroup GroupVertical(in DisplayName name, params DeviceUiGroup[] content) 
        => GroupVertical(name, content.AsEnumerable());

    public static DeviceUiGroup GroupVertical(string name, params DeviceUiGroup[] content)
        => GroupVertical(DisplayName.From(name), content);

    public static DeviceUiGroup GroupVertical(string name, IEnumerable<DeviceUiGroup> content) 
        => GroupVertical(DisplayName.From(name), content);
    
    public static DeviceUiGroup GroupVertical(in DisplayName name, IEnumerable<DeviceUiGroup> content) 
        => new(name, UIType.GroupVertical, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty.AddRange(content));
    
    public static DeviceUiGroup Sensor(DeviceSensor sensor)
        => new(sensor.DisplayName, GetUIType(sensor.SensorType), sensor.Identifer, ImmutableList<DeviceUiGroup>.Empty);

    private static UIType GetUIType(SensorType sensorType)
        => sensorType switch
        {
            SensorType.Double => UIType.SensorDouble,
            SensorType.String => UIType.SensorString,
            SensorType.Number => UIType.SensorNumber,
            _ => throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, "Unsupportet Sensor Type Value"),
        };

    public static DeviceUiGroup Text(in DisplayName text) 
        => new(text, UIType.Text, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty);
    
    public static DeviceUiGroup Text(string text) 
        => Text(DisplayName.From(text));

    public static DeviceUiGroup TextInput(DeviceId id, in DisplayName label, in DisplayName text) 
        => new(label, UIType.Input, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty);
    
    public static DeviceUiGroup TextInput(DeviceId id, string label, string text) 
        => TextInput(id, DisplayName.From(label), DisplayName.From(text));
    
    public static DeviceUiGroup TextInput(DeviceId id, in DisplayName label) 
        => TextInput(id, label, DisplayName.From(string.Empty));
    
    public static DeviceUiGroup TextInput(DeviceId id, string label) 
        => TextInput(id, label, string.Empty);
    
    public static DeviceUiGroup Button(DeviceButton button)
        => new(button.DisplayName, UIType.Button, button.Identifer, ImmutableList<DeviceUiGroup>.Empty);

    public static DeviceUiGroup Empty() => new(DisplayName.From(""), UIType.Group, DeviceId.New, ImmutableList<DeviceUiGroup>.Empty);
}