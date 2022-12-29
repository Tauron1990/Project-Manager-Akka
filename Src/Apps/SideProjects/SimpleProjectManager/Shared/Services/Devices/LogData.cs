using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record LogData(LogLevel LogLevel, LogCategory Category, SimpleMessage Message, DateTime Occurance, ImmutableDictionary<PropertyName, PropertyValue> Data);