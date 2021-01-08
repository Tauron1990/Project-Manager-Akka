using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Model
{
    public abstract class ViewModelActorRef : IViewModel
    {
        public abstract IActorRef Actor { get; }
        public abstract Type ModelType { get; }
        public abstract bool IsInitialized { get; }
        public abstract void AwaitInit(Action waiter);
        internal abstract void Init(IActorRef actor);
    }

    [PublicAPI]
    public sealed class ViewModelActorRef<TModel> : ViewModelActorRef, IViewModel<TModel>
        where TModel : UiActor
    {
        private IActorRef _actor = ActorRefs.Nobody;
        private bool _isInitialized;

        private List<Action>? _waiter = new();

        public override IActorRef Actor => _actor;

        public override Type ModelType => typeof(TModel);

        public override bool IsInitialized => _isInitialized;

        public override void AwaitInit(Action waiter)
        {
            lock (this)
            {
                if (IsInitialized)
                    waiter();
                else
                    _waiter!.Add(waiter);
            }
        }

        internal override void Init(IActorRef actor)
        {
            Interlocked.Exchange(ref _actor, actor);

            lock (this)
            {
                _isInitialized = true;
                foreach (var action in _waiter!)
                    action();

                _waiter = null;
            }
        }
    }
}