using System.Runtime.Serialization;
using Akkatecture.Jobs;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record DatabaseFile(
    [property:DataMember, MemoryPackOrder(0)]ProjectFileId Id, 
    [property:DataMember, MemoryPackOrder(1)]FileName Name, 
    [property:DataMember, MemoryPackOrder(2)]FileSize Size, 
    [property:DataMember, MemoryPackOrder(3)]string JobName);