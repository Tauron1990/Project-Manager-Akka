namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public sealed class MgiLoggingConfiguration
{
    public int Port { get; set; } = MgiMachine.Port;

    public string Ip { get; set; } = "127.0.0.1";
}