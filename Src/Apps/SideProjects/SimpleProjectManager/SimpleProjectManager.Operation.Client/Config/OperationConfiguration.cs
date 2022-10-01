using System.Collections.Immutable;
using FluentValidation.Results;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed record OperationConfiguration(
    string ServerIp, int ServerPort, int AkkaPort, bool ImageEditor, bool Device, string MachineInterface,
    string Name, string Path)
{
    public OperationConfiguration()
        : this(string.Empty, -1, -1, false, false, string.Empty, string.Empty, string.Empty){}

    public string ServerUrl
        => $"http://{ServerIp}:{ServerPort}";

    public string AkkaUrl
        => $"akka.tcp://SimpleProjectManager-Server@{ServerIp}:{AkkaPort}";
    
    public async ValueTask<ImmutableList<ValidationFailure>> Validate()
    {
        var validator = new ConfigurationValidator();
        var validationResult = await validator.ValidateAsync(this);

        return !validationResult.IsValid ? validationResult.Errors.ToImmutableList() : ImmutableList<ValidationFailure>.Empty;
    }
}