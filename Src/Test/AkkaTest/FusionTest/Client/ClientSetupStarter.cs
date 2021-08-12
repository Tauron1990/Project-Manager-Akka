using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.FusionTest.Data;
using AkkaTest.FusionTest.Helper;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Tauron.Application.AkkaNode.Bootstrap;

namespace AkkaTest.FusionTest.Client
{
    public class ClientSetupStarter : IStartUpAction
    {
        private readonly ActorSystem              _system;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IClaimManager _claimManager;

        public ClientSetupStarter(ActorSystem system, IHostApplicationLifetime lifetime, IClaimManager claimManager)
        {
            _system        = system;
            _lifetime = lifetime;
            _claimManager = claimManager;
        }
        
        public async void Run()
        {
            if (await ConnectionHelper.Connect(_system, _lifetime))
            {
                AnsiConsole.Clear();
                AnsiConsole.Render(new Rule("Verbinndung Hergestellt"));
                AnsiConsole.WriteLine();
                var clientType = AnsiConsole.Prompt(
                    new SelectionPrompt<(Props Props, string Name)>()
                       .UseConverter(p => p.Name)
                       .AddChoices(
                            (Props.Create(() => new DisplayActor(_claimManager)), "Display"),
                            (Props.Create(() => new EditActor(_claimManager)), "Editor")));

                _system.ActorOf(clientType.Item1, clientType.Item2);
            }
        }
    }
}