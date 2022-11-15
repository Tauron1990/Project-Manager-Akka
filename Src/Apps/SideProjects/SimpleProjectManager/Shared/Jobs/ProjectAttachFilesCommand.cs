using System.Collections.Immutable;

namespace SimpleProjectManager.Shared;

public sealed record ProjectAttachFilesCommand(ProjectId Id, ProjectName Name, ImmutableList<ProjectFileId> Files);