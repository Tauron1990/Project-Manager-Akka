﻿using System.Collections.Immutable;
using FluentValidation.Results;
using Newtonsoft.Json;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed record OperationConfiguration(
    Port SelfPort,
    ServerIp ServerIp, Port ServerPort, Port AkkaPort, ObjectName Name, DeviceData Device, EditorData Editor)
{
    public OperationConfiguration()
        : this(Port.Empty, ServerIp.Empty, Port.Empty, Port.Empty, ObjectName.Empty, new DeviceData(), new EditorData()) { }

    public DeviceId CreateDeviceId(string id)
        => DeviceId.ForName($"{id}--{Name.Value}");

    [JsonIgnore]
    public string ServerUrl
        => $"http://{ServerIp}:{ServerPort}";

    [JsonIgnore]
    public string AkkaUrl
        => $"akka.tcp://SimpleProjectManager-Server@{ServerIp}:{AkkaPort}";

    public async ValueTask<ImmutableList<ValidationFailure>> Validate()
    {
        var validator = new ConfigurationValidator();
        ValidationResult? validationResult = await validator.ValidateAsync(this).ConfigureAwait(false);

        return !validationResult.IsValid ? validationResult.Errors.ToImmutableList() : ImmutableList<ValidationFailure>.Empty;
    }
}