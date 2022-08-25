using MongoDB.Driver;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed record Updater<TData>(UpdateDefinition<TData> UpdateDefinition) : IUpdate<TData>;