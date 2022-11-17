using Akkatecture.Jobs;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed record TaskManagerDeleteEntry(string EntryId) : IJob;