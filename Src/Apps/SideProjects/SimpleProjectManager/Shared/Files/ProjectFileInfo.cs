using System.Runtime.Serialization;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record ProjectFileInfo(
    [property:DataMember, MemoryPackOrder(0)]ProjectFileId Id, 
    [property:DataMember, MemoryPackOrder(1)]ProjectName ProjectName, 
    [property:DataMember, MemoryPackOrder(2)]FileName FileName, 
    [property:DataMember, MemoryPackOrder(3)]FileSize Size, 
    [property:DataMember, MemoryPackOrder(4)]FileType FileType, 
    [property:DataMember, MemoryPackOrder(5)]FileMime Mime);