using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using Stl.Async;
using Stl.Fusion;
using Tauron;

namespace ServiceManager.Server.AppCore.Helper
{
    public class AppIpService : IAppIpManager, IDisposable
    {
        private readonly IInternalAppIpManager _appIpManager;
        private readonly IDisposable _subscription;

        public AppIpService(IInternalAppIpManager appIpManager)
        {
            _appIpManager = appIpManager;
            _subscription = _appIpManager.IpChanged.Subscribe(
                _ => { using (Computed.Invalidate()) GetIp().Ignore(); });
        }

        public virtual Task<string> WriteIp(WriteIpCommand command, CancellationToken token)
            => _appIpManager.WriteIp(command.Ip);

        public virtual Task<AppIp> GetIp()
            => Task.FromResult(_appIpManager.Ip);

        public void Dispose()
            => _subscription.Dispose();
    }
}