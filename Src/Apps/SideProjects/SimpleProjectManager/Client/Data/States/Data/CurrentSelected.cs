using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed record CurrentSelected(JobSortOrderPair? Pair, JobData? JobData);