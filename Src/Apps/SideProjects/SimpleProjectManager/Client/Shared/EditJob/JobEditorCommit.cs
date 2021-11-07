using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.EditJob;

public sealed record JobEditorPair<TData>(TData NewData, TData? OldData);

public sealed record JobEditorCommit(JobEditorPair<JobData> JobData);