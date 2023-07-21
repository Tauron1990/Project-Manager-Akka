using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed partial class UiConfiguration : TauronProfile
{
    private readonly ILogger<UiConfiguration> _logger;
    private readonly SemaphoreSlim _lock = new(1);
    
    public UiConfiguration(ITauronEnviroment tauronEnviroment, ILogger<UiConfiguration> logger) :
        base("Simple_Project_Manager_Client", tauronEnviroment.DefaultProfilePath)
    {
        _logger = logger;
    }

    [LoggerMessage(1, LogLevel.Error, "Error on Save MGI UI Configuration")]
    private partial void ErrorOnSaveProfile(Exception e);
    
    public string UvLampIp
    {
        get => GetValue(string.Empty);
        set => SetVaue(value);
    }

    public async Task RunSave()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        
        try
        {
            await Task.Run(Save).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ErrorOnSaveProfile(e);
        }
        finally
        {
            _lock.Release();
        }
    }
}