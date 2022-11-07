using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record Jobs(ImmutableList<JobInfo> JobInfos);