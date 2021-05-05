using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Newtonsoft.Json;
using Serilog;
using ServiceHost.Installer;
using Servicemnager.Networking.Data;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands;

namespace ServiceHost
{
    public static class Program
    {
        private const string MonitorName = @"Global\Tauron.Application.ProjectManagerHost";

        static async Task Main(string[] args)
        {
            bool createdNew = false;

            using var m = await Task.Factory.StartNew(() => new Mutex(true, MonitorName, out createdNew), CancellationToken.None, 
                TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, MutexThread.Inst);
            try
            {
                if (createdNew)
                {
                    await StartApp(args);
                }
                else
                {
                    try
                    {
                        await using var client = new NamedPipeClientStream(".", MonitorName, PipeDirection.In, PipeOptions.Asynchronous);
                        await client.ConnectAsync(10000, CancellationToken.None);
                        
                        byte[] data = Encoding.UTF8.GetBytes(new ExposedCommandLineProvider(args).Serialize());
                        var (message, lenght) = new NetworkMessageFormatter(MemoryPool<byte>.Shared)
                           .WriteMessage(NetworkMessage.Create("Args", data));
                        using var mem = message;

                        await client.WriteAsync(mem.Memory[..lenght]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
                    }
                }
            }
            finally
            {
                if (createdNew)
                {
                    await Task.Factory.StartNew(() => m.ReleaseMutex(), CancellationToken.None, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, MutexThread.Inst);
                    MutexThread.Inst.Dispose();
                }
                IncomingCommandHandler.Handler?.Stop();
            }
        }

        private static async Task StartApp(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Host, IpcApplicationType.Server)
               .ConfigureAutoFac(cb =>
                {
                    cb.RegisterType<CommandHandlerStartUp>().As<IStartUpAction>();
                    cb.RegisterModule<HostModule>();
                })
               .ConfigurateAkkaSystem((context, system) =>
                {
                    var cluster = Cluster.Get(system);

                    #if TEST
                    cluster.Join(cluster.SelfAddress);
                    #endif

                    cluster.RegisterOnMemberRemoved(() => system.Terminate());
                    cluster.RegisterOnMemberUp(()
                        => ServiceRegistry.GetRegistry(system).RegisterService(new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress)));
                })
               .Build().Run();
        }

        private sealed class ExposedCommandLineProvider : CommandLineConfigurationProvider
        {
            public ExposedCommandLineProvider(IEnumerable<string> args) : base(args) { }

            public string Serialize()
            {
                Load();
                return JsonConvert.SerializeObject(Data);
            }
        }

        private sealed class CommandHandlerStartUp : IStartUpAction
        {
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;

            public CommandHandlerStartUp(Func<IConfiguration, ManualInstallationTrigger> installTrigger) 
                => _installTrigger = installTrigger;

            public void Run() 
                => IncomingCommandHandler.SetHandler(new IncomingCommandHandler(_installTrigger));
        }

        public sealed class IncomingCommandHandler
        {
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;
            
            private readonly NamedPipeServerStream _reader = new(MonitorName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            private readonly CancellationTokenSource _cancellationToken = new();
            
            private readonly MessageBuffer _buffer = new(MemoryPool<byte>.Shared);
            private readonly List<IMemoryOwner<byte>> _incomming = new();

            public IncomingCommandHandler(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
            {
                _installTrigger = installTrigger;
                Task.Factory.StartNew(async () => await Reader(), TaskCreationOptions.LongRunning);
            }

            private async Task Reader()
            {

                try
                {
                    while (true)
                    {
                        await _reader.WaitForConnectionAsync(_cancellationToken.Token);
                        var messageNotRead = true;

                        while (messageNotRead)
                        {
                            var mem = MemoryPool<byte>.Shared.Rent();
                            var amount = await _reader.ReadAsync(mem.Memory);
                            if (amount == 0)
                            {
                                mem.Dispose();
                                continue;
                            }

                            _incomming.Add(mem);
                            var msg = _buffer.AddBuffer(mem.Memory);
                            if(msg == null) continue;

                            switch (msg.Type)
                            {
                                case "Args":
                                    messageNotRead = false;
                                    ParseAndRunData(Encoding.UTF8.GetString(msg.Data));
                                    break;
                            }
                        }

                        foreach (var owner in _incomming) owner.Dispose();
                        _incomming.Clear();

                        //using var buffer = MemoryPool<byte>.Shared.Rent(4);
                        //if (!await TryRead(buffer, 4, _cancellationToken.Token)) continue;

                        //var count = BitConverter.ToInt32(buffer.Memory.Span);
                        //using var dataBuffer = MemoryPool<byte>.Shared.Rent(count);

                        //if (await TryRead(buffer, count, _cancellationToken.Token)) 
                        //    ParseAndRunData(Encoding.UTF8.GetString(dataBuffer.Memory[..count].Span));
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Error on Read CommandLine from outer process");
                }

                await _reader.DisposeAsync();
                Handler = null;
            }

            //private async Task<bool> TryRead(IMemoryOwner<byte> buffer, int lenght, CancellationToken token)
            //{
            //    var currentLenght = 0;

            //    while (true)
            //    {
            //        if (_reader.IsMessageComplete)
            //            return currentLenght == lenght;
            //        if (currentLenght > lenght)
            //            return false;

            //        currentLenght = await _reader.ReadAsync(buffer.Memory.Slice(currentLenght, lenght - currentLenght), token);

            //        if (currentLenght == lenght)
            //            return _reader.IsMessageComplete;
            //    }
            //}

            private void ParseAndRunData(string rawdata)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawdata);
                var config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();

                _installTrigger(config).Run();
            }

            public void Stop() 
                => _cancellationToken.Cancel();

            public static void SetHandler(IncomingCommandHandler handler)
            {
                if(Handler != null)
                    handler.Stop();
                else
                    Handler = handler;
            }

            public static IncomingCommandHandler? Handler { get; private set; }
        }

        public sealed class MutexThread : TaskScheduler, IDisposable
        {
            public static readonly MutexThread Inst = new();

            private readonly BlockingCollection<Task> _tasks = new();

            private MutexThread()
            {
                Thread thread = new(() =>
                                    {
                                        foreach (var task in _tasks.GetConsumingEnumerable()) 
                                            TryExecuteTask(task);

                                        _tasks.Dispose();
                                    });

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        #pragma warning disable CA1416 // Plattformkompatibilität überprüfen
                        thread.SetApartmentState(ApartmentState.STA);
                        #pragma warning restore CA1416 // Plattformkompatibilität überprüfen
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                thread.IsBackground = true;

                thread.Start();
            }

            protected override IEnumerable<Task> GetScheduledTasks() => _tasks;

            protected override void QueueTask(Task task) 
                => _tasks.Add(task);

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (taskWasPreviouslyQueued)
                    return false;

                _tasks.Add(task);

                return false;
            }

            public void Dispose()
            {
                _tasks.CompleteAdding();
            }
        }
    }
}
