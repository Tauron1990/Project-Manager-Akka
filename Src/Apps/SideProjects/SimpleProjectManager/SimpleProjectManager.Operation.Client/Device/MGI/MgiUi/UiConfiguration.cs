using System.Net.NetworkInformation;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace SimpleProjectManager.Operation.Client.Device.MGI.MgiUi;

public sealed class UiConfiguration : TauronProfile
{
    private readonly object _sync = new();

    public UiConfiguration(ITauronEnviroment tauronEnviroment) :
        base("Simple_Project_Manager_Client", tauronEnviroment.DefaultProfilePath)
    {
        PropertyChangedObservable.Subscribe(RunSave);
    }

    public string Ip
    {
        get => GetValue(string.Empty);
        set => SetVaue(value);
    }

    private void RunSave(string _)
    {
        #pragma warning disable EPC13
        Task.Run(Save);
        #pragma warning restore EPC13
    }
}