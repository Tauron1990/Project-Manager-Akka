using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class TaskSource<TData> : ISourceBlock<TData>
    {
        private ISourceBlock<TData> _block;

        public TaskSource(Task<ISourceBlock<TData>> factory)
        {
            if (factory.IsCompletedSuccessfully)
                _block = factory.Result;
            _block = new TaskBlock(factory, this);
        }

        public void Complete() => _block.Complete();

        public void Fault(Exception exception) => _block.Fault(exception);

        public Task Completion => _block.Completion;

        public TData? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) => _block.ConsumeMessage(messageHeader, target, out messageConsumed);

        public IDisposable LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions) => _block.LinkTo(target, linkOptions);

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => _block.ReleaseReservation(messageHeader, target);

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => _block.ReserveMessage(messageHeader, target);

        private void SetSource(ISourceBlock<TData> source) => _block = source;

        private void SwapLinks(IEnumerable<Link> links)
        {
            foreach (var (target, options, disposable) in links) 
                disposable.Disposable = _block.LinkTo(target, options);
        }

        private void LinkCompletion(TaskCompletionSource<Unit> completion) 
            => _block.Completion.ContinueWith(t =>
                                              {
                                                  if (t.IsCompletedSuccessfully)
                                                      completion.TrySetResult(Unit.Default);
                                                  else if(t.IsCanceled)
                                                      completion.SetCanceled();
                                                  else
                                                    completion.SetException(t.Exception!.InnerExceptions);
                                              });

        private sealed class TaskBlock : ISourceBlock<TData>
        {
            private ImmutableList<Link> _links = ImmutableList<Link>.Empty;
            private readonly TaskCompletionSource<Unit> _completion = new();
            private bool _compledted;
            private readonly object _lock = new();

            public TaskBlock(Task<ISourceBlock<TData>> task, TaskSource<TData> source)
            {
                task.ContinueWith(t =>
                                  {
                                      var target = t.IsCompletedSuccessfully
                                          ? t.Result
                                          : new FaultedBlock(t);

                                      source.SetSource(target);
                                      if (_compledted)
                                          target.Complete();
                                      lock (_lock)
                                      {
                                          source.SwapLinks(_links);
                                          if (!_compledted && target is not FaultedBlock)
                                              source.LinkCompletion(_completion);
                                      }
                                  });
            }

            public void Complete()
            {
                lock (_lock)
                {
                    _completion.TrySetResult(Unit.Default);
                    _compledted = true;
                }
            }

            public void Fault(Exception exception) => Complete();

            public Task Completion => _completion.Task;

            public TData? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed)
            {
                messageConsumed = false;
                return default;
            }

            public IDisposable LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions)
            {
                SingleAssignmentDisposable disposable = new();
                lock (_lock)
                    _links = _links.Add(new Link(target, linkOptions, disposable));
                return disposable;
            }

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target)
            { }

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target)
                => false;
        }

        private sealed class FaultedBlock : ISourceBlock<TData>
        {
            private readonly Task<ISourceBlock<TData>> _fault;

            public FaultedBlock(Task<ISourceBlock<TData>> fault) => _fault = fault;

            public void Complete() => _fault.GetAwaiter().GetResult().Complete();

            public void Fault(Exception exception) => _fault.GetAwaiter().GetResult().Fault(exception);

            public Task Completion => _fault.GetAwaiter().GetResult().Completion;

            public TData? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) 
                => _fault.GetAwaiter().GetResult().ConsumeMessage(messageHeader, target, out messageConsumed);

            public IDisposable LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions) 
                => _fault.GetAwaiter().GetResult().LinkTo(target, linkOptions);

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) 
                => _fault.GetAwaiter().GetResult().ReleaseReservation(messageHeader, target);

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target)
                => _fault.GetAwaiter().GetResult().ReserveMessage(messageHeader, target);
        }

        private sealed record Link(ITargetBlock<TData> Target, DataflowLinkOptions Options, SingleAssignmentDisposable Disposable);
    }
}