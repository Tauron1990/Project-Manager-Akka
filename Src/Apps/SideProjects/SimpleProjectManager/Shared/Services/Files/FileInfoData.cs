namespace SimpleProjectManager.Shared.Services;

public sealed record FileInfoData
{
    public ProjectFileId Id { get; init; } = ProjectFileId.New;

    public ProjectName ProjectName { get; init; } = ProjectName.Empty;

    public FileName FileName { get; init; } = FileName.Empty;

    public FileSize Size { get; init; } = FileSize.Empty;

    public FileType FileType { get; init; }

    public FileMime Mime { get; init; } = FileMime.Generic;
}