using System.Net.NetworkInformation;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed class UiConfiguration : TauronProfile
{
    private readonly object _sync = new();
    
    public UiConfiguration(ITauronEnviroment tauronEnviroment) :
        base("Simple_Project_Manager_Client", tauronEnviroment.DefaultProfilePath) { }

    public string Ip { get; set; }

}