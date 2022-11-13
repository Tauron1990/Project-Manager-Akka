using Akkatecture.ValueObjects;

namespace Akkatecture.Jobs;

public abstract class SchedulerMessage<TJob, TIdentity> : ValueObject
    where TJob : IJob
    where TIdentity : IJobId { }