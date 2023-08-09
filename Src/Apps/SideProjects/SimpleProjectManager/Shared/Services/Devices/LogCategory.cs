using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct LogCategory(
    [property:DataMember, MemoryPackOrder(0)] string Value);