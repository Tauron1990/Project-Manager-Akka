using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace AkkaTest
{
    internal static class Program
    {
        private static async Task Main()
        {
            using var system = ActorSystem.Create("TestSystem");

            var test = system.ActorOf(TellAliveFeature.New());

            var testResult = await QueryIsAlive.Ask(system, test, Timeout.InfiniteTimeSpan);

            Console.ReadKey();
        }
    }
}