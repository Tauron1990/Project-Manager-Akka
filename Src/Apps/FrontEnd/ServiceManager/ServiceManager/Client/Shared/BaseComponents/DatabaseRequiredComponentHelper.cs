using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion;

namespace ServiceManager.Client.Shared.BaseComponents
{
    public sealed class DatabaseRequiredComponentHelper : IDisposable
    {
        private readonly Subject<Unit> _changed = new();

        public DatabaseRequiredComponentHelper(IDatabaseConfig config, IStateFactory stateFactory)
        {
            IsReady = stateFactory.NewComputed(new ComputedState<bool>.Options(), (_, _) => config.GetIsReady());
            IsReady.AddEventHandler(StateEventKind.All, (_, _) => _changed.OnNext(Unit.Default));
        }

        public IObservable<Unit> OnChanged => _changed.AsObservable();

        public IState<bool> IsReady { get; }

        public void Dispose()
            => _changed.Dispose();
    }
}