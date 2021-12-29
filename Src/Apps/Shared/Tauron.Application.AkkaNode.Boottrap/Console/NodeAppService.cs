using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Tauron.Application.AkkaNode.Bootstrap.Console;

public sealed class NodeAppService : IHostedService
{
    private IEnumerable<IStartUpAction> _actions;
    private IServiceScope _serviceScope;
    
    public NodeAppService(IServiceProvider provider)
    {
        _serviceScope = provider.CreateScope();
        _actions = _serviceScope.ServiceProvider.GetRequiredService<IEnumerable<IStartUpAction>>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Maximize();

        return Task.Run(
            () =>
            {
                foreach (var startUpAction in _actions)
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
            },
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serviceScope.Dispose();
        _serviceScope = null!;
        _actions = null!;
        return Task.CompletedTask;
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

    private static void Maximize()
    {
        Process p = Process.GetCurrentProcess();
        ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
    }
}