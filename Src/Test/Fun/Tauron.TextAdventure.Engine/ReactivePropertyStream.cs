namespace Tauron.TextAdventure.Engine;


#pragma warning disable MA0097
public class ReactivePropertyStream<T> : ReactiveProperty<T>
    #pragma warning restore MA0097
{
        private readonly IDisposable _streamSubscription;

        public ReactivePropertyStream(IObservable<T> stream) : base(default!)
        {
            _streamSubscription = stream.Subscribe(z => Value = z);
        }

        #pragma warning disable CA1816
        public override void Dispose()
            #pragma warning restore CA1816
        {
            _streamSubscription.Dispose();
            base.Dispose();
        }
    }