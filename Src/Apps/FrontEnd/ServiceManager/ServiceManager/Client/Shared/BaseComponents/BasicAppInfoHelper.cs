using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion;

namespace ServiceManager.Client.Shared.BaseComponents
{
    public sealed class BasicAppInfoHelper
    {
        private readonly Subject<Unit> _ipSubject = new();

        private readonly Subject<Unit> _dataSubject = new();

        public IState<AppIp> Ip { get; }

        public IObservable<Unit> IpChanged => _ipSubject.AsObservable();

        public IState<AppData> AppData { get; }

        public IObservable<Unit> AppDataChanged => _dataSubject.AsObservable();
        
        public BasicAppInfoHelper(IClusterConnectionTracker tracker, IAppIpManager ipManager, IDatabaseConfig databaseConfig, IStateFactory stateFactory)
        {
            Ip = stateFactory.NewComputed(new ComputedState<AppIp>.Options() ,(_, _) => ipManager.GetIp());
            AppData = stateFactory.NewComputed(
                new ComputedState<AppData>.Options(),
                async (_, _) => new AppData(await tracker.GetIsConnected(), await tracker.GetIsSelf(), await databaseConfig.GetIsReady()));
            
            Ip.AddEventHandler(StateEventKind.All, Handler);
            AppData.AddEventHandler(StateEventKind.All, Handler);
        }

        private void Handler(IState<AppData> arg1, StateEventKind arg2)
            => _dataSubject.OnNext(Unit.Default);

        private void Handler(IState<AppIp> arg1, StateEventKind arg2)
            => _ipSubject.OnNext(Unit.Default);
    }
}