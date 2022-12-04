using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine;

namespace SpaceConqueror;

public static class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = await GameHost.Create<Game>(args).ConfigureAwait(false);

        await host.RunAsync().ConfigureAwait(false);
    }
}