using System.Collections.Immutable;
using FluentValidation.Results;
using SimpleProjectManager.Client.Operations.Shared;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed record OperationConfiguration(
    ServerIp ServerIp, Port ServerPort, Port AkkaPort, ObjectName Name, DeviceData Device, EditorData Editor)
{
    public OperationConfiguration()
        : this(ServerIp.Empty, Port.Empty, Port.Empty, ObjectName.Empty,  new DeviceData(), new EditorData()) { }

    public string ServerUrl
        => $"http://{ServerIp}:{ServerPort}";

    public string AkkaUrl
        => $"akka.tcp://SimpleProjectManager-Server@{ServerIp}:{AkkaPort}"; 

    public async ValueTask<ImmutableList<ValidationFailure>> Validate()
    {
        var validator = new ConfigurationValidator();
        ValidationResult? validationResult = await validator.ValidateAsync(this).ConfigureAwait(false);

        return !validationResult.IsValid ? validationResult.Errors.ToImmutableList() : ImmutableList<ValidationFailure>.Empty;
    }
}