using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using SimpleProjectManager.Operation.Client;
using Spectre.Console.Cli;

namespace ClientRunnerApp;

public sealed class ClientApp : AsyncCommand<ClientApp.Stettings>
{
    public sealed class Stettings : CommandSettings
    {
        [Description("Erzwingt den aufruf des Setups")]
        [CommandOption("-s|--setup")]
        [DefaultValue(false)]
        public bool ForceSetup { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Stettings settings)
    {
        IHost host = await new ClientRunner(new ConsoleInteraction())
           .CreateClient(new ClientConfiguration(Environment.GetCommandLineArgs(), settings.ForceSetup))
           .ConfigureAwait(false);

        await host.RunAsync().ConfigureAwait(false);
        
        return 0;
    }
}