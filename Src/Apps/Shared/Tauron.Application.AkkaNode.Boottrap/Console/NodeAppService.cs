using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Tauron.Application.AkkaNode.Bootstrap.Console;

public sealed class NodeAppService : IHostedService
{
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Maximize();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

    private static void Maximize()
    {
        var p = Process.GetCurrentProcess();
        if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
    }
}