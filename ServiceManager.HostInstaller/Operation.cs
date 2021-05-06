using ServiceManager.HostInstaller.Phase;
using ServiceManager.HostInstaller.Phases;

namespace ServiceManager.HostInstaller
{
    public static class Operation
    {
        private static PhaseManager<OperationContext> CreateManager()
            => new(
                new SetConfigAndConnectPhase(),
                new TryGetDataPhase(),
                new ExtractAndInstallPhase(),
                new SelfDestroyPhase());

        public static void Start(OperationContext context) 
            => CreateManager().RunNext(context);
    }
}