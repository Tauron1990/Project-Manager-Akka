using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public sealed record JobSortOrderPair(SortOrder Order, JobInfo Info);