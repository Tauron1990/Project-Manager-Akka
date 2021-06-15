using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class SelectManeyBlock<TData, TInput> : ISourceBlock<TData>
    {
        private readonly Func<TInput, ISourceBlock<TData>> _select;
        private readonly IPropagatorBlock<TData, TData> _output = new BufferBlock<TData>();
        private readonly WatchDog _watchDog;
        private readonly IDisposable _input;

        public SelectManeyBlock(ISourceBlock<TInput> incomming, Func<TInput, ISourceBlock<TData>> select)
        {
            _select = @select;
            _watchDog = new WatchDog(incomming, OnCompled);

            var action = new ActionBlock<TInput>(NewBlock, new ExecutionDataflowBlockOptions
                                                           {
                                                               BoundedCapacity = ActionTask.ThreadCount,
                                                               MaxDegreeOfParallelism = ActionTask.ThreadCount
                                                           });

            _input = incomming.LinkTo(action, new DataflowLinkOptions {PropagateCompletion = true});
        }

        private void OnCompled(Exception? exception)
        {
            if (exception == null)
                _output.Complete();
            else
                _output.Fault(exception);
        }

        private void NewBlock(TInput input)
        {
            var block = _select(input);
            _watchDog.Add(block, block.LinkTo(_output));
        }

        void IDataflowBlock.Complete()
        {
            _input.Dispose();
            _watchDog.Compled();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            _input.Dispose();
            _watchDog.Fault(exception);
        }

        Task IDataflowBlock.Completion => _watchDog.Completion;

        TData? ISourceBlock<TData>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) 
            => _output.ConsumeMessage(messageHeader, target, out messageConsumed);

        IDisposable ISourceBlock<TData>.LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions)
            => _output.LinkTo(target, linkOptions);

        void ISourceBlock<TData>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) 
            => _output.ReleaseReservation(messageHeader, target);

        bool ISourceBlock<TData>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) 
            => _output.ReserveMessage(messageHeader, target);

        private sealed class WatchDog
        {
            private readonly Action<Exception?> _onCompled;
            private readonly TaskCompletionSource<Unit> _completion = new();

            private readonly List<Exception> _exceptions = new();
            private readonly List<IDisposable> _links = new();

            private int _running = 1;
            private int _finish;

            public Task Completion => _completion.Task;

            public WatchDog(ISourceBlock<TInput> start, Action<Exception?> onCompled)
            {
                _onCompled = onCompled;
                start.Completion.ContinueWith(OnCompled);
            }

            private void OnCompled(Task? t)
            {
                if (t != null)
                {
                    lock (this)
                    {
                        if (t.IsCanceled)
                            _exceptions.Add(new TaskCanceledException(t));
                        else if (t.IsFaulted && t.Exception != null)
                            _exceptions.AddRange(t.Exception.InnerExceptions);
                    }
                }

                Interlocked.Increment(ref _finish);
                if (Interlocked.Decrement(ref _running) > 0) return;

                lock (this)
                {
                    if (_exceptions.Count != 0)
                    {
                        _completion.TrySetException(_exceptions);
                        _onCompled(new AggregateException(_exceptions));
                    }
                    else
                    {
                        _completion.TrySetResult(Unit.Default);
                        _onCompled(null);
                    }
                }
            }

            public void Add(ISourceBlock<TData> source, IDisposable link)
            {
                lock (this)
                {
                    if (_finish != 0) return;

                    Interlocked.Increment(ref _running);

                    _links.Add(link);
                    source.Completion.ContinueWith(t =>
                                                   {
                                                       lock (this)
                                                           _links.Remove(link);
                                                       OnCompled(t);
                                                   });
                }
            }

            public void Fault(Exception e)
            {
                lock (this)
                    _exceptions.Add(e);

                Compled();
            }

            public void Compled()
            {
                lock (this)
                {
                    Interlocked.Increment(ref _finish);
                    foreach (var disposable in _links) disposable.Dispose();
                    _running = int.MinValue + 1000;
                }

                OnCompled(null);
            }
        }
    }
}