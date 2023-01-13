using System;
using System.Globalization;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using NLog;
using Tauron.Application.AkkaNode.Bootstrap.IpcMessages;

namespace Tauron.Application.AkkaNode.Bootstrap;

[UsedImplicitly]
public sealed class KillHelper
{
    [UsedImplicitly]
    #pragma warning disable IDE0052 // Ungelesene private Member entfernen
    private static KillHelper? _keeper;
    #pragma warning restore IDE0052 // Ungelesene private Member entfernen

    private readonly string? _comHandle;
    private readonly IpcConnection _ipcConnection;
    private readonly ILogger _logger;

    private readonly ActorSystem _system;

    public KillHelper(IConfiguration configuration, ActorSystem system, IIpcConnection ipcConnection)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _comHandle = configuration["ComHandle"];
        _system = system;
        _ipcConnection = (IpcConnection)ipcConnection;

        _keeper = this;
        system.RegisterOnTermination(
            () =>
            {
                _ipcConnection.Disconnect();
                _keeper = null;
            });
    }

    public void Run()
    {
        if(string.IsNullOrWhiteSpace(_comHandle)) return;

        string? errorToReport = null;
        if(_ipcConnection.IsReady)
        {
            _ipcConnection.Start(_comHandle);
            if(!_ipcConnection.IsReady)
                errorToReport = _ipcConnection.ErrorMessage;
        }
        else
        {
            errorToReport = _ipcConnection.ErrorMessage;
        }

        if(!string.IsNullOrWhiteSpace(errorToReport))
        {
            _logger.Warn(CultureInfo.InvariantCulture, "Error on Start Kill Watch: {Error}", errorToReport);

            return;
        }

        _ipcConnection.OnMessage<KillNode>()
           .Subscribe(
                _ => _system.Terminate(),
                exception => _logger.Error(exception, "Error On Killwatch Message Recieve"));
    }
}