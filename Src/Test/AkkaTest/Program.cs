﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Stl.Fusion;
using Stl.Fusion.Extensions;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Application.Master.Commands.KillSwitch;

namespace AkkaTest
{
    internal static class Program
    {
        public static readonly string ExeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        
        private static async Task Main(string[] args)
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            
            string seedPath = Path.Combine(Program.ExeFolder, AkkaConfigurationBuilder.Main);

            var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync(seedPath));

            baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {localIP}").WithFallback(baseConfig);

            await File.WriteAllTextAsync(seedPath, baseConfig.ToString(true));
            
            await SetupRunner.Run(Host.CreateDefaultBuilder(args))
                             .ConfigureServices(
                                  s =>
                                  {
                                      s.AddFusion()
                                       .AddFusionTime()
                                       .AddSandboxedKeyValueStore()
                                       .AddInMemoryKeyValueStore();
                                  })
                             .Build().RunAsync();
        }

    }
}