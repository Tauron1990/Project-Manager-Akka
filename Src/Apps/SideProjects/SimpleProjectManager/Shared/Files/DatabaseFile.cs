using Akkatecture.Jobs;

namespace SimpleProjectManager.Shared;

public sealed record DatabaseFile(ProjectFileId Id, FileName Name, FileSize Size, JobName JobName);