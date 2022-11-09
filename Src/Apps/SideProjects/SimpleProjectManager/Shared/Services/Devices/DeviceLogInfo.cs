using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceLogInfo(SimpleMessage Messages, ImmutableDictionary<PropertyName, PropertyValue> Propertys);