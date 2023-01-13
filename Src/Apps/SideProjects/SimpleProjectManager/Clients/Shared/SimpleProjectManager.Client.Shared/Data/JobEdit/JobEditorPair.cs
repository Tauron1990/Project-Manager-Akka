namespace SimpleProjectManager.Client.Shared.Data.JobEdit;

public sealed record JobEditorPair<TData>(TData NewData, TData? OldData);