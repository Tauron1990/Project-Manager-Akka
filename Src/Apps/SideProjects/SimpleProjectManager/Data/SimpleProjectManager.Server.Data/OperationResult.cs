namespace SimpleProjectManager.Server.Data;

public sealed record OperationResult(bool IsAcknowledged, long ModifiedCount);