using System.Collections.Immutable;

namespace SimpleProjectManager.Shared;

public sealed record ProjectAttachFilesCommand(ProjectId Id, ImmutableList<ProjectFileId> Files);

public sealed record ProjectRemoveFilesCommand(ProjectId Id, ImmutableList<ProjectFileId> Files);