using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record CurrentSelected(JobSortOrderPair? Pair, JobData? JobData);