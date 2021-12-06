namespace SimpleProjectManager.Operation.Client.Shared.Config;

public sealed record OperationConfiguration(string ServerIp, bool ImageEditor, bool Device, string Name)
{
    public OperationConfiguration()
        : this(string.Empty, false, false, string.Empty){}
}