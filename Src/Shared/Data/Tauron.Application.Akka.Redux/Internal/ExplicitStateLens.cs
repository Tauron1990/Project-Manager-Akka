using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;
using Reactive.Streams;

namespace Tauron.Application.Akka.Redux.Internal;

internal class ExplicitStateLens<TState, TFeatureState> : BaseStateLens<TState, TFeatureState>
{
    private sealed class ContextShape : GraphStage<FlowShape<DispatchedAction<TState>, TState>>
    {
        private sealed class Logic : GraphStageLogic, ISubscriber<TFeatureState>
        {
            private readonly ContextShape _shape;
            private TState? _pending;
            private ISubscription? _subscription;
            
            public Logic(ContextShape shape) : base(shape.Shape)
            {
                _shape = shape;
                
                shape._processor.Subscribe(this);
                
                SetHandler(_shape.Incomming,
                    () =>
                    {
                        try
                        {
                            var data = Grab(_shape.Incomming);
                            _pending = data.State;
                            var newData = new DispatchedAction<TFeatureState>(_shape._stateLens._featureSelector(_pending), data.Action);
                            _shape._processor.OnNext(newData);
                        }
                        catch (Exception e)
                        {
                            _shape._processor.OnError(e);
                        }
                    });
            }

            public override void PreStart()
            {
                Pull(_shape.Incomming);
                base.PreStart();
            }

            public override void PostStop()
            {
                _subscription?.Cancel();
                _shape._processor.OnComplete();
                base.PostStop();
            }

            public void OnSubscribe(ISubscription subscription)
                => _subscription = subscription;

            public void OnNext(TFeatureState element)
            {

                if(_pending is null)
                {
                    Fail(_shape.Outgoinng, new InvalidOperationException("No Inital state befor Processing is Provided"));
                    return;
                }
                
                try
                {
                    _pending = _shape._stateLens._stateReducer(_pending, element);
                    Emit(_shape.Outgoinng, _pending, () => Pull(_shape.Incomming));
                }
                catch (Exception e)
                {
                    _shape._processor.OnError(e);
                }
            }

            public void OnError(Exception cause)
                => Fail(_shape.Outgoinng, cause);

            public void OnComplete()
                => CompleteStage();
        }
        
        private readonly IProcessor<DispatchedAction<TFeatureState>, TFeatureState> _processor;
        private readonly ExplicitStateLens<TState, TFeatureState> _stateLens;

        private Inlet<DispatchedAction<TState>> Incomming { get; } = new("StateLens.In");

        private Outlet<TState> Outgoinng { get; } = new("StateLens.Out");

        public ContextShape(IProcessor<DispatchedAction<TFeatureState>, TFeatureState> processor, ExplicitStateLens<TState, TFeatureState> stateLens)
        {
            _processor = processor;
            _stateLens = stateLens;
        }
        
        public override FlowShape<DispatchedAction<TState>, TState> Shape => new(Incomming, Outgoinng);
        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes)
            => new Logic(this);
    }
    
    private readonly Func<TState, TFeatureState> _featureSelector;

    private readonly Func<TState, TFeatureState, TState> _stateReducer;
    private readonly IMaterializer _materializer;

    public ExplicitStateLens(
        Func<TState, TFeatureState> featureSelector, 
        Func<TState, TFeatureState, TState> stateReducer,
        IMaterializer materializer)
    {
        _featureSelector = featureSelector;
        _stateReducer = stateReducer;
        _materializer = materializer;
    }

    protected override On<TState> CreateParentReducer(On<TFeatureState> on)
    {
        var processor = on.Mutator.ToProcessor().Run(_materializer);

        return new On<TState>(
            Flow.FromGraph<DispatchedAction<TState>, TState, NotUsed>(
                GraphDsl.Create(b => b.Add(new ContextShape(processor, this)))),
            on.ActionType);
    }
}