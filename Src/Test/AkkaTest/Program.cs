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
            var uri = new Uri("akka.tcp://Project-Manager@192.168.105.96:59169");
            Console.ReadKey();
        }
    }
}