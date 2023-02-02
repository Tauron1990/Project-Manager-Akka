using System;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tauron.Application.AkkaNode.Bootstrap.IpcMessages;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tauron.Application.AkkaNode.Bootstrap;

[UsedImplicitly]
public sealed partial class KillHelper
{
    [UsedImplicitly]
    #pragma warning disable IDE0052 // Ungelesene private Member entfernen
    private static KillHelper? _keeper;
    #pragma warning restore IDE0052 // Ungelesene private Member entfernen

    private readonly string? _comHandle;
    private readonly IpcConnection _ipcConnection;

    private readonly ActorSystem _system;
    private readonly ILogger<KillHelper> _logger;

    public KillHelper(IConfiguration configuration, ActorSystem system, IIpcConnection ipcConnection, ILogger<KillHelper> logger)
    {
        _comHandle = configuration["ComHandle"];
        _system = system;
        _logger = logger;
        _ipcConnection = (IpcConnection)ipcConnection;

        _keeper = this;
        system.RegisterOnTermination(
            () =>
            {
                _ipcConnection.Disconnect();
                _keeper = null;
            });
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Error on Start Kill Watch: {errorToReport}")]
    private partial void ErrorStartKillWatch(string errorToReport);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Error on Killwatch Recieve")]
    private partial void ErrorOnMessage(Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Critical, Message = "Error on Initialize Killwatch for {comHandle}")]
    private partial void InitializationError(Exception ex, string? comHandle);
    
    public async void Run()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(_comHandle)) return;

            string? errorToReport = null;
            if(_ipcConnection.IsReady)
            {
                await _ipcConnection.Start(_comHandle).ConfigureAwait(false);
                if(!_ipcConnection.IsReady)
                    errorToReport = _ipcConnection.ErrorMessage;
            }
            else
            {
                errorToReport = _ipcConnection.ErrorMessage;
            }

            if(!string.IsNullOrWhiteSpace(errorToReport))
            {
                ErrorStartKillWatch(errorToReport);
                return;
            }

            _ipcConnection.OnMessage<KillNode>()
               .Subscribe(_ => _system.Terminate(), ErrorOnMessage);
        }
        catch (Exception e)
        {
            InitializationError(e, _comHandle);
        }
    }
}