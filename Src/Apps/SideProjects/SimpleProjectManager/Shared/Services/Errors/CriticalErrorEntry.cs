namespace SimpleProjectManager.Shared.Services;

public sealed record CriticalErrorEntry(string Id, CriticalError Error, bool IsDisabled);