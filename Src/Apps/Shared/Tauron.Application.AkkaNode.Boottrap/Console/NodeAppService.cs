﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    public sealed class NodeAppService : IHostedService
    {
        private Owned<IEnumerable<IStartUpAction>>? _actions;

        public NodeAppService(Owned<IEnumerable<IStartUpAction>> startUpActions) => _actions = startUpActions;
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Maximize();
            return Task.Run(() =>
                            {
                                if (_actions != null)
                                {
                                    foreach (var startUpAction in _actions.Value)
                                    {
                                        try
                                        {
                                            startUpAction.Run();
                                        }
                                        catch (Exception e)
                                        {
                                            LogManager.GetCurrentClassLogger().Error(e, "Error on Startup Action");
                                        }
                                    }
                                }

                                _actions?.Dispose();
                                _actions = null;
                            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}