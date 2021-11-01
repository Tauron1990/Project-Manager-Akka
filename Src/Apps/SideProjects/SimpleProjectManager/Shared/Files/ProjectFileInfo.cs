
namespace SimpleProjectManager.Shared;

public sealed record ProjectFileInfo(ProjectFileId Id, ProjectName ProjectName, FileName FileName, FileSize Size, FileType FileType, FileMime Mime);