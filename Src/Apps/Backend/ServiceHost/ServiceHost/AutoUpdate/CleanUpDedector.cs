using Microsoft.Extensions.Configuration;

namespace ServiceHost.AutoUpdate
{
    public sealed class CleanUpDedector
    {
        private readonly int _id;
        private readonly bool _switch;
        private readonly IAutoUpdater _updater;

        public CleanUpDedector(IConfiguration configuration, IAutoUpdater updater)
        {
            _updater = updater;
            _switch = configuration.GetValue("cleanup", false);
            _id = configuration.GetValue("id", -1);
        }

        public void Run()
        {
            if (!_switch) return;

            _updater.Tell(new StartCleanUp(_id));
        }
    }
}