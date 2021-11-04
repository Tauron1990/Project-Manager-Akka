using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record JobData(ProjectId Id, ProjectName JobName, ProjectStatus Status, SortOrder? Ordering, ProjectDeadline? Deadline, ImmutableList<ProjectFileId> ProjectFiles);