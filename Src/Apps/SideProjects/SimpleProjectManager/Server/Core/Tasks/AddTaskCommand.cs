using Akkatecture.Jobs;
using Akkatecture.Jobs.Commands;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed record AddTaskCommand(string Name, object Command, string Info)
{
    public static AddTaskCommand Create<TJob, TId>(string name, SchedulerCommand<TJob, TId> command)
        where TJob : IJob where TId : IJobId
        => new(name, Prepare(command), GetInfo(command));

    private static SchedulerCommand<TJob, TId> Prepare<TJob, TId>(SchedulerCommand<TJob, TId> command)
        where TJob : IJob where TId : IJobId
        => command switch
        {
            ScheduleCron<TJob, TId> cron => cron.WithAck(OperationResult.Success()).WithNack(OperationResult.Failure()),
            ScheduleRepeatedly<TJob, TId> repeatedly => repeatedly.WithAck(OperationResult.Success()).WithNack(OperationResult.Failure()),
            Schedule<TJob, TId> schedule => schedule.WithAck(OperationResult.Success()).WithNack(OperationResult.Failure()),
            _ => throw new InvalidOperationException("Invalid SchedulerCommand")
        };
    
    private static string GetInfo<TJob, TId>(SchedulerCommand<TJob, TId> command)
        where TJob : IJob where TId : IJobId
        => command switch
        {
            ScheduleCron<TJob, TId> cron => $"Cron Task: {cron.CronExpression}",
            ScheduleRepeatedly<TJob, TId> repeatedly => $"Datum: {repeatedly.TriggerDate:G} -- Regelmäsig: {repeatedly.Interval:g}",
            Schedule<TJob, TId> schedule => $"Start Dtaum: {schedule.TriggerDate:G}",
            _ => throw new InvalidOperationException("Invalid SchedulerCommand")
        };
}