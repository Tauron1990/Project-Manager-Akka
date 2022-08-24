namespace SimpleProjectManager.Server.Data;

public sealed record DbOperationResult(bool IsAcknowledged, int ModifiedCount, int DeletedCount);