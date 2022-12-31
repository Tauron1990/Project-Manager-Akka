using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace SimpleProjectManager.Shared.Services.Devices;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed record LogData(LogLevel LogLevel, LogCategory Category, SimpleMessage Message, DateTime Occurance, ImmutableDictionary<string, PropertyValue> Data);