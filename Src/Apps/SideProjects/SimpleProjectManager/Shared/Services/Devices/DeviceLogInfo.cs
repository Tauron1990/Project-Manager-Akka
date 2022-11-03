using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Devices;

public sealed record DeviceLogInfo(string Messages, ImmutableDictionary<string, string> Propertys);