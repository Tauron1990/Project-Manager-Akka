using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow;

[PublicAPI]
public abstract class WorkflowActorBase<TStep, TContext> : ActorBase, IWithTimers
    where TStep : IStep<TContext> where TContext : IWorkflowContext
{
    private readonly Dictionary<Type, Delegate> _signals = new();
    private readonly Dictionary<Type, Delegate> _starter = new();

    private readonly Dictionary<StepId, StepRev<TStep, TContext>> _steps = new();
    private readonly object _timeout = new();

    private string _errorMessage = string.Empty;
    private ChainCall? _lastCall;

    private Action<WorkflowResult<TContext>>? _onFinish;

    private bool _running;
    private IActorRef? _starterSender;
    private bool _waiting;
    protected ILoggingAdapter Log { get; } = Context.GetLogger();
    protected TContext RunContext { get; private set; } = default!;

    public ITimerScheduler Timers { get; set; } = default!;

    protected void SetError(string error)
    {
        _errorMessage = error;
    }

    protected override bool Receive(object message)
    {
        if(_running)
            return _waiting ? Singnaling(message) : Running(message);

        return Initializing(message);
    }

    protected virtual bool Singnaling(object msg)
    {
        try
        {
            if(msg is TimeoutMarker)
            {
                _errorMessage = "Timeout";
                Finish(isok: false);

                return true;
            }

            if(!_signals.TryGetValue(msg.GetType(), out Delegate? del)) return false;

            Timers.Cancel(_timeout);

            if(del.DynamicInvoke(RunContext, msg) is not StepId id)
                throw new InvalidOperationException("Invalid Call of Signal Delegate");

            Self.Tell(new ChainCall(id).WithBase(_lastCall), _starterSender);

            _lastCall = null;

            return true;
        }
        finally
        {
            _waiting = false;
        }
    }

    protected virtual bool Initializing(object msg)
    {
        switch (msg)
        {
            case ChainCall:
            case LoopElement:
                return true;
            case WorkflowResult<TContext> result:
                _onFinish?.Invoke(result);

                return true;
        }

        if(!_starter.TryGetValue(msg.GetType(), out Delegate? del)) return false;

        del.DynamicInvoke(msg);

        return true;
    }

    protected void Signal<TMessage>(Func<TContext, TMessage, StepId> signal)
    {
        _signals[typeof(TMessage)] = signal;
    }

    protected void StartMessage<TType>(Action<TType> msg)
    {
        _starter[typeof(TType)] = msg;
    }

    protected virtual bool Running(object msg)
    {
        try
        {
            return msg switch
            {
                ChainCall chain => ProcessChainCall(chain),
                LoopElement(var stepRev, var chainCall) => ProcessLoopElement(stepRev, chainCall),
                _ => false,
            };
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Exception While Processing Workflow");
            _errorMessage = exception.Message;
            Finish(isok: false);

            return true;
        }
    }

    private bool ProcessLoopElement(StepRev<TStep, TContext> stepRev, ChainCall chainCall)
    {
        StepId loopId = stepRev.Step.NextElement(RunContext);

        if(loopId != StepId.LoopEnd)
            Self.Forward(new LoopElement(stepRev, chainCall));

        if(loopId == StepId.LoopContinue)
            return true;

        if(string.Equals(loopId.Name, StepId.Fail.Name, StringComparison.Ordinal))
        {
            Finish(isok: false);

            return true;
        }

        ProgressConditions(stepRev, baseCall: chainCall);

        return true;
    }

    private bool ProcessChainCall(ChainCall chain)
    {
        StepId id = chain.Id;
        if(id == StepId.Fail)
        {
            Finish(isok: false);

            return true;
        }

        if(!_steps.TryGetValue(id, out var rev))
        {
            Log.Warning("No Step Found {Id}", id.Name);
            _errorMessage = id.Name;
            Finish(isok: false);

            return true;
        }

        StepId sId = rev.Step.OnExecute(RunContext);

        switch (sId.Name)
        {
            case "Fail":
                _errorMessage = rev.Step.ErrorMessage;
                Finish(isok: false);

                break;
            case "None":
                ProgressConditions(rev, finish: true, chain);

                return true;
            case "Loop":
                Self.Forward(new LoopElement(rev, chain));

                return true;
            case "Finish":
            case "Skip":
                Finish(isok: true, rev);

                break;
            case "Waiting":
                _waiting = true;
                if(rev.Step is IHasTimeout { Timeout: { } } timeout)
                    Timers.StartSingleTimer(_timeout, new TimeoutMarker(), timeout.Timeout.Value);
                _lastCall = chain;

                return true;
            default:
                Self.Forward(new ChainCall(sId).WithBase(chain));

                return true;
        }

        if(_running)
            Self.Forward(chain.Next());

        return true;
    }

    private void ProgressConditions(StepRev<TStep, TContext> rev, bool finish = false, ChainCall? baseCall = null)
    {
        var std = (from con in rev.Conditions
                   let stateId = con.Select(rev.Step, RunContext)
                   where !string.Equals(stateId.Name, StepId.None.Name, StringComparison.Ordinal)
                   select stateId).ToArray();

        if(std.Length != 0)
        {
            Self.Forward(new ChainCall(std).WithBase(baseCall));

            return;
        }

        if(rev.GenericCondition is null)
        {
            if(finish)
                Finish(isok: false);
        }
        else
        {
            StepId cid = rev.GenericCondition.Select(rev.Step, RunContext);
            if(!string.Equals(cid.Name, StepId.None.Name, StringComparison.Ordinal))
                Self.Forward(new ChainCall(cid).WithBase(baseCall));
        }
    }

    protected void OnFinish(Action<WorkflowResult<TContext>> con)
    {
        _onFinish = _onFinish.Combine(con);
    }

    private void Finish(bool isok, StepRev<TStep, TContext>? rev = null)
    {
        _starterSender = null;
        _running = false;
        if(isok)
            rev?.Step.OnExecuteFinish(RunContext);
        Self.Forward(new WorkflowResult<TContext>(isok, _errorMessage, RunContext));
        RunContext = default!;
        _errorMessage = string.Empty;
    }

    public void Start(TContext context)
    {
        _starterSender = Sender;
        _running = true;
        RunContext = context;
        Self.Forward(new ChainCall(StepId.Start));
    }

    protected StepConfiguration<TStep, TContext> WhenStep(StepId id, TStep step)
    {
        var rev = new StepRev<TStep, TContext>(step);
        _steps[id] = rev;

        return new StepConfiguration<TStep, TContext>(rev);
    }

    private sealed record TimeoutMarker;

    private sealed record LoopElement(StepRev<TStep, TContext> Rev, ChainCall Call);

    private sealed class ChainCall
    {
        private ChainCall(StepId[] stepIds, int position, ChainCall? baseCall = null)
        {
            BaseCall = baseCall;
            StepIds = stepIds;
            Position = position;
        }

        internal ChainCall(StepId id, ChainCall? baseCall = null)
        {
            BaseCall = baseCall;
            StepIds = new[] { id };
            Position = 0;
        }

        internal ChainCall(StepId[] ids)
        {
            StepIds = ids;
            Position = 0;
        }

        private ChainCall? BaseCall { get; }

        private StepId[] StepIds { get; }

        private int Position { get; }

        internal StepId Id => Position >= StepIds.Length ? BaseCall?.Id ?? StepId.Fail : StepIds[Position];

        internal ChainCall Next()
        {
            int newPos = Position + 1;

            if(newPos == StepIds.Length && BaseCall != null) return BaseCall.Next();

            return new ChainCall(StepIds, newPos);
        }

        internal ChainCall WithBase(ChainCall? call)
        {
            if(call is null) return this;

            call = call.Next();

            return new ChainCall(call.StepIds, call.Position, this);
        }
    }
}