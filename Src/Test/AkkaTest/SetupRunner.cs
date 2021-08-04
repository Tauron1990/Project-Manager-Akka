using System;
using AkkaTest.FusionTest.Client;
using AkkaTest.FusionTest.Seed;
using AkkaTest.FusionTest.Server;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Tauron.Application.AkkaNode.Bootstrap;

namespace AkkaTest
{
    public static class SetupRunner
    {

        public static IHostBuilder Run(IHostBuilder builder)
        {
            Console.Title = "Initial Setup";
            AnsiConsole.Render(new Rule("Starte Setup"));
            AnsiConsole.WriteLine();
            
            return AnsiConsole.Prompt(CreatePrompt()) switch
            {
                SetupType.Seed => SeedSetup.Run(builder),
                SetupType.Server => ServerSetup.Run(builder),
                SetupType.Client => ClientSetup.Run(builder),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IPrompt<SetupType> CreatePrompt()
            => new SelectionPrompt<SetupType>()
               .Title("Bitte Modus wählen")
              .UseConverter(t => t switch
               {
                   SetupType.Seed => "Seed und Logger",
                   SetupType.Server => "Server",
                   SetupType.Client => "Client",
                   _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
               })
              .AddChoices(SetupType.Seed, SetupType.Server, SetupType.Client);

        public enum SetupType
        {
            Seed,
            Server,
            Client
        }
    }
}