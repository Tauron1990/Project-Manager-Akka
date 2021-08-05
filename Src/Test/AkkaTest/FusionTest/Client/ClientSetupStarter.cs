using Akka.Actor;
using AkkaTest.FusionTest.Data;
using AkkaTest.FusionTest.Helper;
using Microsoft.Extensions.Hosting;
using Tauron.Application.AkkaNode.Bootstrap;

namespace AkkaTest.FusionTest.Client
{
    public class ClientSetupStarter : IStartUpAction
    {
        private readonly ActorSystem              _system;
        private readonly IHostApplicationLifetime _lifetime;

        public ClientSetupStarter(ActorSystem system, IHostApplicationLifetime lifetime, IClaimManager claimManager)
        {
            _system        = system;
            _lifetime = lifetime;
        }
        
        public async void Run()
        {
            if (await ConnectionHelper.Connect(_system, _lifetime))
            {
                
            }
        }
    }
}