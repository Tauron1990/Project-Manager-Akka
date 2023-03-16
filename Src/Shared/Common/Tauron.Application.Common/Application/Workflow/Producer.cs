using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tauron.Application.Workflow;

[PublicAPI]
public abstract class Producer<TState, TContext>
    where TState : IStep<TContext>
    where TContext : notnull
{
    private readonly Dictionary<StepId, StepRev<TState, TContext>> _states;

    private string _errorMessage = string.Empty;

    private StepId _lastId;

    protected Producer() => _states = new Dictionary<StepId, StepRev<TState, TContext>>();

    public void Begin(StepId id, TContext context)
    {
        if(!Process(id, context))
            throw new InvalidOperationException("Procession not Successful");

        if(string.Equals(_lastId.Name, StepId.Fail.Name, StringComparison.Ordinal))
            throw new InvalidOperationException(_errorMessage);
    }

    [DebuggerStepThrough]
    protected bool SetLastId(StepId id)
    {
        _lastId = id;

        return string.Equals(_lastId.Name, StepId.Finish.Name, StringComparison.Ordinal)
            || string.Equals(_lastId.Name, StepId.Fail.Name, StringComparison.Ordinal);
    }

    protected virtual bool Process(StepId id, TContext context)
    {
        if(SetLastId(id)) return true;

        if(!_states.TryGetValue(id, out var rev))
            return SetLastId(StepId.Fail);

        StepId sId = rev.Step.OnExecute(context);
        bool result;

        switch (sId.Name)
        {
            case "Fail":
                _errorMessage = rev.Step.ErrorMessage;

                return SetLastId(sId);
            case "None":
                result = ProgressConditions(rev, context);

                break;
            case "Loop":
                if(ProcessLoop(context, rev, out result))
                    return result;

                break;
            case "Finish":
            case "Skip":
                result = true;

                break;
            default:
                return SetLastId(StepId.Fail);
        }

        if(!result)
            rev.Step.OnExecuteFinish(context);

        return result;
    }

    private bool ProcessLoop(TContext context, StepRev<TState, TContext> rev, out bool result)
    {
        var ok = true;

        do
        {
            StepId loopId = rev.Step.NextElement(context);
            if(string.Equals(loopId.Name, StepId.LoopEnd.Name, StringComparison.Ordinal))
            {
                ok = false;

                continue;
            }

            if(string.Equals(loopId.Name, StepId.Fail.Name, StringComparison.Ordinal))
            {
                result = SetLastId(StepId.Fail);

                return true;
            }

            #pragma warning disable GU0011
            ProgressConditions(rev, context);
            #pragma warning restore GU0011
        } while (ok);

        result = false;

        return false;
    }

    private bool ProgressConditions(StepRev<TState, TContext> rev, TContext context)
    {
        var std = (from con in rev.Conditions
                   let stateId = con.Select(rev.Step, context)
                   where !string.Equals(stateId.Name, StepId.None.Name, StringComparison.Ordinal)
                   select stateId).ToArray();

        if(std.Length != 0) return std.Any(id => Process(id, context));

        if(rev.GenericCondition is null) return false;

        StepId cid = rev.GenericCondition.Select(rev.Step, context);

        return !string.Equals(cid.Name, StepId.None.Name, StringComparison.Ordinal) && Process(cid, context);
    }

    public StepConfiguration<TState, TContext> SetStep(StepId id, TState stade)
    {
        var rev = new StepRev<TState, TContext>(stade);
        _states[id] = rev;

        return new StepConfiguration<TState, TContext>(rev);
    }

    public StepConfiguration<TState, TContext> GetStateConfiguration(StepId id) => new(_states[id]);
}