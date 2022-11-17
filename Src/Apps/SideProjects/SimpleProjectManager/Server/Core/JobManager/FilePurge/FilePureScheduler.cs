using Akkatecture.Jobs;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed class FilePureScheduler : JobScheduler<FilePureScheduler, FilePurgeJob, FilePurgeId> { }