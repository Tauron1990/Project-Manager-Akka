using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NLog;
using ServiceHost.Client.Shared;
using ServiceHost.Installer;
using Servicemnager.Networking.Data;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace ServiceHost
{
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
            await Bootstrap.StartNode(
                    args,
                    KillRecpientType.Host,
                    IpcApplicationType.Server,
                    ab =>
                    {
                        ab.ConfigureAutoFac(
                                cb =>
                                {
                                    cb.RegisterType<CommandHandlerStartUp>().As<IStartUpAction>();
                                    cb.RegisterModule<HostModule>();
                                })
                           .ConfigureAkkaSystem(
                                (context, system) =>
                                {
                                    system.RegisterOnTermination(exitManager.RegularExit);
                                    var cluster = Cluster.Get(system);

                                    #if TEST
                                    cluster.Join(cluster.SelfAddress);
                                    #endif

                                    cluster.RegisterOnMemberRemoved(
                                        () =>
                                        {
                                            exitManager.MemberExit();
                                            system.Terminate()
                                               .Ignore();
                                        });
                                    cluster.RegisterOnMemberUp(
                                        () => ServiceRegistry.Get(system)
                                           .RegisterService(
                                                new RegisterService(
                                                    context.HostingEnvironment.ApplicationName,
                                                    cluster.SelfUniqueAddress,
                                                    ServiceTypes.ServideHost)));
                                });
                    },
                    consoleLog: true)
               .Build().RunAsync();
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

        private sealed class CommandHandlerStartUp : IStartUpAction
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

        private sealed class ExitManager
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

        public sealed class IncomingCommandHandler
        {
            private readonly MessageBuffer _buffer = new(MemoryPool<byte>.Shared);
            private readonly CancellationTokenSource _cancellationToken = new();
            private readonly List<IMemoryOwner<byte>> _incomming = new();
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;

            private readonly NamedPipeServerStream _reader = new(MonitorName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            public IncomingCommandHandler(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
            {
                _installTrigger = installTrigger;
                Task.Factory.StartNew(async () => await Reader(), TaskCreationOptions.LongRunning)
                   .Ignore();
            }

            public static IncomingCommandHandler? Handler { get; private set; }

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

                            if (msg == null) continue;

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
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Error(e, "Error on Read CommandLine from outer process");
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
                if (Handler != null)
                    handler.Stop();
                else
                    Handler = handler;
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