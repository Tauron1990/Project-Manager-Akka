using System.Collections.Immutable;

namespace SimpleProjectManager.Shared;

public sealed record CreateProjectCommand(ProjectName Project, ImmutableList<ProjectFileId> Files, ProjectStatus Status, ProjectDeadline? Deadline)
{ }