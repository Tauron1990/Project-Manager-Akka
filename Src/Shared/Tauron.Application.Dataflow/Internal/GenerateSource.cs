using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class GenerateSource<TState, TResult> : ISourceBlock<TResult>
    {
        private TState _state;
        private readonly Func<TState, bool> _condition;
        private readonly Func<TState, TState> _iterate;
        private readonly Timer? _timer;
        private readonly object _lock = new();
        private readonly IPropagatorBlock<TState, TResult> _block;

        public GenerateSource(TState state, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, TimeSpan? interval)
        {
            _state = state;
            _condition = condition;
            _iterate = iterate;

            if (interval != null)
                _timer = new Timer(TimerNext, null, interval.Value, interval.Value);

            _block = TransformBlock.Create(resultSelector);
            _block.Completion.ContinueWith(t => _timer?.Dispose());
        }

        private void TimerNext(object? state)
        {
            if (!_condition(_state))
            {
                _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _block.Complete();
                return;
            }

            if (MoveNext()) return;
            
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void StartLoop()
        {
            while (_condition(_state))
                if (!MoveNext())
                    return;

            _block.Complete();
        }

        private bool MoveNext()
        {
            try
            {
                lock (_lock)
                {
                    _state = _iterate(_state);
                    _block.Post(_state);
                }

                return true;
            }
            catch (Exception e)
            {
                _block.Fault(e);
                return false;
            }
        }

        public ISourceBlock<TResult> Start()
        {
            if(_timer == null)
                Task.Run(StartLoop);

            return this;
        }

        void IDataflowBlock.Complete() => _block.Complete();

        void IDataflowBlock.Fault(Exception exception) => _block.Fault(exception);

        Task IDataflowBlock.Completion => _block.Completion;

        TResult? ISourceBlock<TResult>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TResult> target, out bool messageConsumed) => _block.ConsumeMessage(messageHeader, target, out messageConsumed);

        IDisposable ISourceBlock<TResult>.LinkTo(ITargetBlock<TResult> target, DataflowLinkOptions linkOptions) => _block.LinkTo(target, linkOptions);

        void ISourceBlock<TResult>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TResult> target) => _block.ReleaseReservation(messageHeader, target);

        bool ISourceBlock<TResult>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TResult> target) => _block.ReserveMessage(messageHeader, target);
    }
}