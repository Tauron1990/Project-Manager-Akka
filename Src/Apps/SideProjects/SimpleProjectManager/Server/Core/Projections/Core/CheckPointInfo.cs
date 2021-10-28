using MongoDB.Bson.Serialization.Attributes;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public class CheckPointInfo
{
    [BsonId] 
    public string Id { get; set; } = string.Empty;

    public long Checkpoint { get; set; } = 0;
}