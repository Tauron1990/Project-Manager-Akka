using System;
using System.Reactive;
using System.Reactive.Linq;
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
using Microsoft.Extensions.Hosting;
using Stl.Fusion;
using Stl.Fusion.AkkaBridge;
using Stl.Fusion.AkkaBridge.Connector;
using Stl.Fusion.Extensions;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Application.Master.Commands.KillSwitch;

namespace AkkaTest
{
    public interface ITestService
    {
        Task<string> Hello(string name);

        Task Tell(string input);
    }

    internal sealed class TestService : ITestService
    {
        public Task<string> Hello(string name)
            => Task.Run(() => $"Hello: {name}");

        public Task Tell(string input)
            => Task.Run(() => Console.WriteLine(input));
    }
    
    internal sealed class TestStart : IStartUpAction
    {
        private readonly ActorSystem _system;
        private readonly IServiceRegistryActor _registryActor;
        private readonly AkkaProxyGenerator _proxyGenerator;

        public TestStart(ActorSystem system, IServiceRegistryActor registryActor, AkkaProxyGenerator proxyGenerator)
        {
            _system = system;
            _registryActor = registryActor;
            _proxyGenerator = proxyGenerator;
            Console.Title = "Test";
        }
        
        public async void Run()
        {
            var cluster = Cluster.Get(_system);
            await cluster.JoinAsync(cluster.SelfAddress);

            var finish = await RegisterTestService();
            await RunTestService();

            finish();
        }

        private async Task<Action> RegisterTestService()
        {
            var serviceTarget = typeof(ITestService);
            var host = ServiceHostActor.CreateHost(_system, new TestService(), serviceTarget);
            
            var response = await _registryActor.RegisterService(new RegisterService(serviceTarget, host), TimeSpan.FromHours(10));
            if(response.Error == null) return () => _registryActor.UnRegisterService(new UnregisterService(host));

            throw response.Error;
        }

        private async Task RunTestService()
        {
            var proxy = (ITestService)_proxyGenerator.GenerateAkkaProxy(typeof(ITestService));

            await proxy.Tell(await proxy.Hello(" World!"));
        }
    }

    internal static class Program
    {
        public static readonly string ExeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .StartNode(KillRecpientType.Seed, IpcApplicationType.NoIpc,
                           b => b.ConfigureAutoFac(c =>
                                                   {
                                                       c.RegisterType<TestStart>().As<IStartUpAction>();
                                                       c.AddAkkaBridge();
                                                   }))
                      .Build().RunAsync();
            
            return;
            
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