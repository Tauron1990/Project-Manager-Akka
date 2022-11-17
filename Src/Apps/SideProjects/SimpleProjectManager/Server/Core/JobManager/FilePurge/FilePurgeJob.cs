using Akkatecture.Jobs;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.JobManager;

public sealed record FilePurgeJob(ProjectFileId FileToDelete) : IJob;