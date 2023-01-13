namespace SimpleProjectManager.Shared.Services;

public sealed record CriticalErrorEntry(ErrorId Id, CriticalError Error, bool IsDisabled);