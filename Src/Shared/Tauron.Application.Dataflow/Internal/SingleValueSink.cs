using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.Util;

namespace Tauron.Application.Dataflow.Internal
{
    internal abstract class SingleValueSink<TInput, TData, TOutput> : Sink<TInput, TOutput>
    {
        private readonly Option<TData> _start;
        private Action? _cancel;

        protected SingleValueSink(Option<TData> start) => _start = start;

        protected void Cancel() => _cancel?.Invoke();

        protected abstract TData Evaluate(TInput input, Option<TData> current);

        protected abstract TOutput ToResult(TData data);

        protected override IPropagatorBlock<TInput, TOutput> CreateBlock(ISourceBlock<TInput> input) 
            => new EvaluationBlock(Evaluate, ToResult, input, _start, action => _cancel = action);

        private sealed class EvaluationBlock : IPropagatorBlock<TInput, TOutput>
        {
            private readonly Func<TInput, Option<TData>, TData> _transform;
            private readonly WriteOnceBlock<TOutput> _outputBlock = new(output => output);

            private readonly object _valueLock = new();
            private Option<TData> _currentOutput;
            private bool _isCompled;

            public void Complete() => _outputBlock.Complete();

            public EvaluationBlock(Func<TInput, Option<TData>, TData> transform, Func<TData, TOutput> outFunc, IDataflowBlock input, Option<TData> start, Action<Action> cancel)
            {
                _transform = transform;
                _currentOutput = start;
                input.Completion.ContinueWith(t =>
                {
                    lock (_valueLock)
                    {
                        try
                        {
                            if (_isCompled) return;
                            _isCompled = true;
                            if (t.IsCompletedSuccessfully)
                            {
                                if (_currentOutput.HasValue)
                                    _outputBlock.Post(outFunc(_currentOutput.Value));
                                else
                                    Fault(new InvalidOperationException("No Elements Recieved"));

                            }
                            else if (t.IsCanceled)
                                Fault(new OperationCanceledException());
                            else
                                Fault(t.Exception.Unwrap() ?? new InvalidOperationException());
                        }
                        catch (Exception e)
                        {
                            Fault(e);
                        }
                    }
                });

                cancel(() => Task.Run(() =>
                                      {
                                          lock (_valueLock)
                                          {
                                              try
                                              {
                                                  if (_isCompled) return;
                                                  if (_currentOutput.HasValue)
                                                      _outputBlock.Post(outFunc(_currentOutput.Value));
                                                  else
                                                      Fault(new InvalidOperationException("No Elements Recieved"));
                                              }
                                              catch (Exception e)
                                              {
                                                  Fault(e);
                                              }
                                          }
                                      }));
            }

            public void Fault(Exception exception) => ((IDataflowBlock)_outputBlock).Fault(exception);

            public Task Completion => _outputBlock.Completion;

            TOutput? ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
                => ((ISourceBlock<TOutput>)_outputBlock).ConsumeMessage(messageHeader, target, out messageConsumed);

            IDisposable ISourceBlock<TOutput>.LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
                => _outputBlock.LinkTo(target, linkOptions);

            void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
                => ((ISourceBlock<TOutput>)_outputBlock).ReleaseReservation(messageHeader, target);

            bool ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
                => ((ISourceBlock<TOutput>)_outputBlock).ReserveMessage(messageHeader, target);

            DataflowMessageStatus ITargetBlock<TInput>.OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput>? source, bool consumeToAccept)
            {
                lock (_valueLock)
                {
                    if (_isCompled) return DataflowMessageStatus.DecliningPermanently;
                    if (consumeToAccept)
                    {
                        if (source == null) return DataflowMessageStatus.NotAvailable;
                        messageValue = source.ConsumeMessage(messageHeader, this, out var ok)!;
                        if (!ok) return DataflowMessageStatus.NotAvailable;
                    }

                    _currentOutput = _transform(messageValue, _currentOutput);
                    return DataflowMessageStatus.Accepted;
                }
            }
        }
    }
}