namespace SimpleProjectManager.Server.Data;

public sealed record DbOperationResult(bool IsAcknowledged, long ModifiedCount, long DeletedCount);