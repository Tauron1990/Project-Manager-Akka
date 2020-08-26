﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SimpleHostSetup.DotnetBuilder;
using SimpleHostSetup.Impl;
using Tauron.Application.Logging;

namespace SimpleHostSetup
{

    //InstallSeed.bat
    //cd %~dp0\TestHost
    //ServiceHost.exe --Install Manual --ZipFile..\Seed.zip --AppName Master-Seed --AppType StartUp
    //pause
    //
    //StartHost.bat
    //cd %~dp0\TestHost
    //ServiceHost.exe
    //pause




    public sealed class Program : IInput
    {
        public static async Task Main()
        {
            Console.Title = "Simple Setup Builder";

            try
            {
                Log.Logger = new LoggerConfiguration().ConfigDefaultLogging("Simple Setup Builder", true).WriteTo.ColoredConsole().CreateLogger();

                string searchStart = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.CodeBase) ?? string.Empty);
                var config = new BuildSystemConfiguration(searchStart, "Project-Manager-Akka.sln", new Program(), () => new DotNetBuilder(), 
                    ImmutableDictionary<string, AppInfo>.Empty.Add("Seed", new AppInfo(AppType.StartUp, "Master.Seed.Node.csproj")), "ServiceHost.csproj");

                var system = new BuildSystem();
                await system.Run(config);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press Key to Close...");
                Console.ReadKey();
            }
        }

        public string GetIp()
        {
            Console.WriteLine();
            Console.Write("Host Ip: ");

            return Console.ReadLine() ?? throw new InvalidOperationException("It was no Ip Read");
        }

        public string GetName(string @for)
        {
            Console.WriteLine();
            Console.Write($"Application Name {@for}: ");

            return Console.ReadLine() ?? throw new InvalidOperationException($"No Application Name for {@for}");
        }

        public IEnumerable<string> GetAppsToInstall(IEnumerable<string> apps) => apps;
    }
}
