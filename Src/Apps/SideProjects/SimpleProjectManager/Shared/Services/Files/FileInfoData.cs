using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record FileInfoData
{
    [DataMember, MemoryPackOrder(0)]
    public ProjectFileId Id { get; init; } = ProjectFileId.New;

    [DataMember, MemoryPackOrder(1)]
    public ProjectName ProjectName { get; init; } = ProjectName.Empty;

    [DataMember, MemoryPackOrder(2)]
    public FileName FileName { get; init; } = FileName.Empty;

    [DataMember, MemoryPackOrder(3)]
    public FileSize Size { get; init; } = FileSize.Empty;

    [DataMember, MemoryPackOrder(4)]
    public FileType FileType { get; init; }

    [DataMember, MemoryPackOrder(5)]
    public FileMime Mime { get; init; } = FileMime.Generic;
}