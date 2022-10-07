using System.Collections.Immutable;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed record DeviceLogInfo(string Messages, ImmutableDictionary<string, string> Propertys);