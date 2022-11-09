using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogData(SimpleMessage Message, DateTime Occurance, ImmutableDictionary<PropertyName, PropertyValue> Data);