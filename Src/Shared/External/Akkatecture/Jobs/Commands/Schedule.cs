// The MIT License (MIT)
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture 
// 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using JetBrains.Annotations;

namespace Akkatecture.Jobs.Commands;

public static class Schedule
{
    public static Schedule<TJob, TId> Fixed<TJob, TId>(TId id, TJob job, DateTime triggerDate)
        where TId : IJobId where TJob : IJob
        => new(id, job, triggerDate);

    public static Schedule<TJob, TId> Repead<TJob, TId>(TId id, TJob job, DateTime triggerDate, TimeSpan interval)
        where TId : IJobId where TJob : IJob
        => new ScheduleRepeatedly<TJob, TId>(id, job, interval, triggerDate);

    public static Schedule<TJob, TId> Cron<TJob, TId>(TId id, TJob job, DateTime triggerDate, string expression)
        where TJob : IJob where TId : IJobId
        => new ScheduleCron<TJob, TId>(id, job, expression, triggerDate);
}

[PublicAPI]
public class Schedule<TJob, TIdentity> : SchedulerCommand<TJob, TIdentity>
    where TJob : IJob
    where TIdentity : IJobId
{
    public Schedule(
        TIdentity jobId,
        TJob job,
        DateTime triggerDate,
        object? ack = null,
        object? nack = null)
        : base(jobId, ack, nack)
    {
        if(job is null) throw new ArgumentNullException(nameof(job));
        if(triggerDate == default) throw new ArgumentException("Triggerdate is default", nameof(triggerDate));

        Job = job;
        TriggerDate = triggerDate;
    }

    public TJob Job { get; }
    public DateTime TriggerDate { get; }

    public virtual Schedule<TJob, TIdentity>? WithNextTriggerDate(DateTime utcDate) => null;

    public virtual Schedule<TJob, TIdentity> WithAck(object? ack) => new(JobId, Job, TriggerDate, ack, Nack);

    public virtual Schedule<TJob, TIdentity> WithNack(object? nack) => new(JobId, Job, TriggerDate, Ack, nack);

    public virtual Schedule<TJob, TIdentity> WithOutAcks() => WithAck(null).WithNack(null);
}