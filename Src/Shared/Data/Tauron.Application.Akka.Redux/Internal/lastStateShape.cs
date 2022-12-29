using System.Collections.Immutable;
using Akka.Streams;
using Akka.Streams.Stage;

namespace Tauron.Application.Akka.Redux.Internal;

public static class LastStateShape
{
    public static LastStateShape<TState, TInput, TOutput> Create<TState, TInput, TOutput>(Func<TState, TInput, TOutput> transform) => new(transform);
}

public sealed class LastStateShape<TState, TInput, TOutput> : GraphStage<FanInShape<TState, TInput, TOutput>>
{
    private readonly Func<TState, TInput, TOutput> _transform;

    public LastStateShape(Func<TState, TInput, TOutput> transform)
    {
        _transform = transform;
        Shape = new FanInShape<TState, TInput, TOutput>(ActionOut, StateIn, ActionIn);
    }

    private Inlet<TState> StateIn { get; } = new("CombineLast.StateIn");

    private Inlet<TInput> ActionIn { get; } = new("CombineLast.ActionIn");

    private Outlet<TOutput> ActionOut { get; } = new("CombineLast.Out");

    public override FanInShape<TState, TInput, TOutput> Shape { get; }

    protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this);

    private sealed class Logic : GraphStageLogic
    {
        private readonly LastStateShape<TState, TInput, TOutput> _holder;
        private TState? _currentState;
        private ImmutableQueue<TInput>? _pending = ImmutableQueue<TInput>.Empty;

        internal Logic(LastStateShape<TState, TInput, TOutput> holder) : base(holder.Shape)
        {
            _holder = holder;

            SetHandler(
                holder.StateIn,
                () =>
                {
                    _currentState = Grab(holder.StateIn);

                    if(_pending is not null)
                        EmitMultiple(
                            holder.ActionOut,
                            Interlocked.Exchange(ref _pending, null)
                               .Select(a => holder._transform(_currentState, a)));

                    Pull(holder.StateIn);
                });

            SetHandler(
                holder.ActionIn,
                () =>
                {
                    if(_pending is not null)
                    {
                        ImmutableInterlocked.Enqueue(ref _pending, Grab(holder.ActionIn));
                        Pull(holder.ActionIn);

                        return;
                    }

                    Emit(
                        holder.ActionOut,
                        holder._transform(_currentState!, Grab(holder.ActionIn)),
                        () => Pull(holder.ActionIn));
                });

            SetHandler(holder.ActionOut, DoNothing);
        }

        public override void PreStart()
        {
            Pull(_holder.StateIn);
            Pull(_holder.ActionIn);
        }
    }
}