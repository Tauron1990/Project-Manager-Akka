using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using NLog;
using ServiceHost.AutoUpdate;
using ServiceHost.ClientApp.Shared;
using ServiceHost.Installer;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Servicemnager.Networking.Data;

namespace ServiceHost
{
    public sealed class IpcMessageFormatter : INetworkMessageFormatter<IDictionary<string, string>>
    {
        private const string BeginMessage = "<start>";
        private const string EndMessage = "<end>";
        public Memory<byte> Header { get; }
        public Memory<byte> Tail { get; }

        public IpcMessageFormatter()
        {
            Header = Encoding.UTF8.GetBytes(BeginMessage);
            Tail = Encoding.UTF8.GetBytes(EndMessage);
        }

        public IDictionary<string, string> ReadMessage(in ReadOnlySequence<byte> bufferMemory) 
            => JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(bufferMemory)) 
               ?? new Dictionary<string, string>(StringComparer.Ordinal);

        public static byte[] Serialize(IEnumerable<string> args)
        {
            var message = $"{BeginMessage}{new ExposedCommandLineProvider(args).Serialize()}{EndMessage}";
            return Encoding.UTF8.GetBytes(message);
        }
            
        private sealed class ExposedCommandLineProvider : CommandLineConfigurationProvider
        {
            internal ExposedCommandLineProvider(IEnumerable<string> args) : base(args) { }

            internal string Serialize()
            {
                Load();

                return JsonConvert.SerializeObject(Data);
            }
        }
    }
    
    public static class Program
    {
        private const string MonitorName = @"Global\Tauron.Application.ProjectManagerHost";

        private static async Task Main(string[] args)
        {
            var exitManager = new ExitManager();
            var createdNew = false;

            using var m = await Task.Factory.StartNew(
                () => new Mutex(initiallyOwned: true, MonitorName, out createdNew),
                CancellationToken.None,
                TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach,
                MutexThread.Inst);
            try
            {
                if (createdNew)
                    await StartApp(args, exitManager);
                else
                    try
                    {
                        await using var client = new NamedPipeClientStream(".", MonitorName, PipeDirection.In, PipeOptions.Asynchronous);
                        await client.ConnectAsync(10000, CancellationToken.None);
                        
                        await client.WriteAsync(IpcMessageFormatter.Serialize(args));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
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

            if (exitManager.NeedRestart)
            {
                using var process = Process.GetCurrentProcess();
                if (process.MainModule?.FileName != null)
                    Process.Start(process.MainModule.FileName).Dispose();
            }
        }

        private static async Task StartApp(string[] args, ExitManager exitManager)
        {
            
            await AppNode.StartNode(
                    args,
                    KillRecpientType.Host,
                    IpcApplicationType.Server,
                    ab =>
                    {
                        ab.ConfigureServices((_, collection) =>
                            {
                                collection.AddSingleton(exitManager);
                                collection.RegisterModule<HostModule>();
                            })
                            .RegisterStartUp<ClusterStartup>(static s => s.Run())
                            .RegisterStartUp<CommandHandlerStartUp>(static s => s.Run())
                            .RegisterStartUp<CleanUpDedector>(static s => s.Run())
                            .RegisterStartUp<ServiceStartupTrigger>(static s => s.Run())
                            .RegisterStartUp<ManualInstallationTrigger>(static s => s.Run());
                    },
                    consoleLog: true)
                .Build()
               .RunAsync().ConfigureAwait(false);
        }

        internal sealed class ClusterStartup
        {
            private readonly ActorSystem _system;
            private readonly ExitManager _exitManager;
            private readonly HostingEnvironment _environment;

            internal ClusterStartup(ActorSystem system, ExitManager exitManager, HostingEnvironment environment)
            {
                _system = system;
                _exitManager = exitManager;
                _environment = environment;
            }

            internal void Run()
            {
                _system.RegisterOnTermination(_exitManager.RegularExit);
                var cluster = Cluster.Get(_system);

                #if TEST
                cluster.Join(cluster.SelfAddress);
                #endif

                cluster.RegisterOnMemberRemoved(
                    () =>
                    {
                        _exitManager.MemberExit();
                        _system.Terminate()
                            .Ignore();
                    });
                cluster.RegisterOnMemberUp(
                    () => ServiceRegistryApi.Get(_system)
                        .RegisterService(
                            new RegisterService(
                                ServiceName.From(_environment.ApplicationName),
                                cluster.SelfUniqueAddress,
                                ServiceTypes.ServideHost)));
            }
        }

        private sealed class CommandHandlerStartUp
        {
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;

            internal CommandHandlerStartUp(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
                => _installTrigger = installTrigger;

            public void Run()
                => IncomingCommandHandler.SetHandler(new IncomingCommandHandler(_installTrigger));
        }

        //private sealed class ExtendedConsoleSink : Target
        //{
        //    private readonly ExtendedConsole _console;

        //    //"[{Timestamp:HH:mm:ss} {Level: u3}]  Message: lj} {NewLine} {Exception}";

        //    public ExtendedConsoleSink(ExtendedConsole console)
        //    {
        //        _console = console;
        //    }

        //    protected override void Write(LogEventInfo logEvent)
        //    {
        //    }

        //    //public void Emit(LogEvent logEvent)
        //    //{
        //    //    string GetLevelText()
        //    //        => logEvent.Level switch
        //    //        {
        //    //            LogEventLevel.Verbose => "VRB",
        //    //            LogEventLevel.Debug => "DGB",
        //    //            LogEventLevel.Information => "INF",
        //    //            LogEventLevel.Warning => "WRN",
        //    //            LogEventLevel.Error => "ERR",
        //    //            LogEventLevel.Fatal => "FTL",
        //    //            _ => throw new ArgumentOutOfRangeException()
        //    //        };

        //    //    Color GetLevelColor()
        //    //        => logEvent.Level switch
        //    //        {
        //    //            LogEventLevel.Verbose => Color.White,
        //    //            LogEventLevel.Debug => Color.Green,
        //    //            LogEventLevel.Information => Color.White,
        //    //            LogEventLevel.Warning => Color.Yellow,
        //    //            LogEventLevel.Error => Color.Red,
        //    //            LogEventLevel.Fatal => Color.DarkRed,
        //    //            _ => throw new ArgumentOutOfRangeException()
        //    //        };


        //    //    _console.Write($"[{logEvent.Timestamp:HH:mm:ss} ");

        //    //    _console.ForegroundColor = GetLevelColor();
        //    //    _console.Write(GetLevelText());
        //    //    _console.ForegroundColor = Color.White;

        //    //    _console.Write("] ");
        //    //    _console.WriteLine(_formatter.Render(logEvent.Properties));

        //    //    if (logEvent.Exception != null)
        //    //        _console.WriteLine(logEvent.Exception.Unwrap()?.ToString());

        //    //}
        //}

        internal sealed class ExitManager
        {
            private bool _regular;

            internal bool NeedRestart { get; private set; }

            internal void MemberExit()
            {
                if (_regular)
                    return;

                NeedRestart = true;
            }

            internal void RegularExit() => _regular = true;
        }

        public sealed class IncomingCommandHandler : IDisposable
        {
            private readonly Channel<IDictionary<string, string>> _provider;
            private readonly MessageReader<IDictionary<string, string>> _messageReader;
            private readonly CancellationTokenSource _cancellationToken = new();
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;

            private readonly NamedPipeServerStream _reader = new(MonitorName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            public IncomingCommandHandler(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
            {
                _installTrigger = installTrigger;

                _provider = Channel.CreateUnbounded<IDictionary<string, string>>();
                _messageReader = new MessageReader<IDictionary<string, string>>(
                    new PipeMessageStream(_reader),
                    new IpcMessageFormatter());
                
                
                
                Task.Factory.StartNew(async () => await Reader(), TaskCreationOptions.LongRunning)
                   .Ignore();
            }

            public static IncomingCommandHandler? Handler { get; private set; }

            private async Task Reader()
            {
                await _reader.WaitForConnectionAsync(_cancellationToken.Token).ConfigureAwait(false);
                
                var readerTask = _messageReader.ReadAsync(_provider.Writer, _cancellationToken.Token);
                
                try
                {
                    await foreach (var data in _provider.Reader.ReadAllAsync(_cancellationToken.Token).ConfigureAwait(false))
                    {
                        var config = new ConfigurationBuilder().AddInMemoryCollection(data!).Build();

                        _installTrigger(config).Run();
                    }
                    
                    await readerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Error(e, "Error on Read CommandLine from outer process");
                }

                await _reader.DisposeAsync().ConfigureAwait(false);
                Handler = null;
            }

            public void Stop()
                => _cancellationToken.Cancel();

            public static void SetHandler(IncomingCommandHandler handler)
            {
                if (Handler != null)
                    handler.Stop();
                else
                    Handler = handler;
            }

            public void Dispose()
            {
                _messageReader.Dispose();
                _cancellationToken.Dispose();
                _reader.Dispose();
            }
        }

        public sealed class MutexThread : TaskScheduler, IDisposable
        {
            public static readonly MutexThread Inst = new();

            private readonly BlockingCollection<Task> _tasks = new();

            private MutexThread()
            {
                Thread thread = new(
                    () =>
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
                }

                thread.IsBackground = true;

                thread.Start();
            }

            public void Dispose()
            {
                _tasks.CompleteAdding();
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
        }
    }
}