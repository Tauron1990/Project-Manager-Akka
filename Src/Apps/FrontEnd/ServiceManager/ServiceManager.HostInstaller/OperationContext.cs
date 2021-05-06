﻿using System;
using System.Threading;
using ServiceManager.HostInstaller.Phases;
using Servicemnager.Networking;
using Servicemnager.Networking.Data;
using Servicemnager.Networking.Server;

namespace ServiceManager.HostInstaller
{
    public sealed class OperationContext : IHasTimeout, IDisposable
    {
        public CancellationTokenSource GlobalTimeout { get; } = new(TimeSpan.FromMinutes(10));

        public ManualResetEventSlim PhaseLock { get; } = new();

        public DataClient DataClient { get; set; }

        public HostConfiguration Configuration { get; set; }

        public string TargetFile { get; set; }

        public Action DestroySelf { get; set; } = () => { };

        public Action<string> WriteLine { get; }

        public Action<string> Write { get; }

        bool IHasTimeout.IsTimeedOut => GlobalTimeout.IsCancellationRequested;

        public OperationContext(Action<string> log)
        {
            WriteLine = s => log(s + Environment.NewLine);
            Write = log;
        }

        public bool ProcessAndWait(NetworkMessage msg, EventHandler<MessageFromServerEventArgs> handler)
        {
            DataClient.OnMessageReceived += handler;

            try
            {
                PhaseLock.Reset();

                DataClient.Send(msg);

                PhaseLock.Wait(GlobalTimeout.Token);
                return !GlobalTimeout.IsCancellationRequested;
            }
            finally
            {
                DataClient.OnMessageReceived -= handler;
            }
        }

        public void Dispose()
        {
            GlobalTimeout.Dispose();
            PhaseLock?.Dispose();
        }
    }
}