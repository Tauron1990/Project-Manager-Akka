using MongoDB.Bson.Serialization.Attributes;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class FileInfoData
{
    [BsonId]
    public ProjectFileId Id { get; set; } = ProjectFileId.New;

    public ProjectName ProjectName { get; set; } = ProjectName.Empty;
    
    public FileName FileName { get; set; } = FileName.Empty;
    
    public FileSize Size { get; set; } = FileSize.Empty;

    public FileType FileType { get; set; }

    public FileMime Mime { get; set; } = FileMime.Generic;
}