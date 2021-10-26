﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Actor;
using Akkatecture.Commands;
using Akkatecture.Core;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core;

public class CommandProcessor
{
    private readonly Guid _nameNamespace = Guid.Parse("587C4D9F-04D2-4180-BD73-544338280714");

    private readonly ActorSystem _system;
    private readonly ILogger<CommandProcessor> _log;
    private readonly CommandMapping[] _commandMappings;
    private readonly ImmutableDictionary<Type, ApiCommandMapping> _apiCommandMappings;
    private readonly ConcurrentDictionary<string, IActorRef> _managers = new();

    public CommandProcessor(ActorSystem system, IEnumerable<CommandMapping> mapping, IEnumerable<ApiCommandMapping> apiCommandMappings, ILogger<CommandProcessor> log)
    {
        _system = system;
        _log = log;
        _commandMappings = mapping.ToArray();
        _apiCommandMappings = apiCommandMappings.ToImmutableDictionary(m => m.TargetType);
    }

    public async Task<IOperationResult> RunCommand(ICommand command)
    {
        var handler = _commandMappings.FirstOrDefault(cm => cm.CommandType.IsInstanceOfType(command));
        if(handler == null) return OperationResult.Failure(new Error("Kein Handler gefunden", "No-Handler"));

        var name = GuidFactories.Deterministic.Create(_nameNamespace, handler.AggregateManager.AssemblyQualifiedName!).ToString("N");
        var manager = _managers.GetOrAdd(name, n => _system.ActorOf(Props.Create(handler.AggregateManager), n));

        try
        {
            return await manager.Ask<IOperationResult>(command, TimeSpan.FromSeconds(30));
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Failure(new Error("Kommando hat zu lange gebraucht", "cmmand-timeout"));
        }
    }

    public async Task<IOperationResult> RunCommand(object apiCommand)
    {
        try
        {
            if (_apiCommandMappings.TryGetValue(apiCommand.GetType(), out var mapping))
                return await RunCommand(mapping.Converter(apiCommand));

            return OperationResult.Failure("Kommando Konverter nicht gefunden", "no-converter");
        }
        catch (Exception e)
        {
            _log.LogError(e, "Error on Process Commando {Command}", apiCommand);
            return OperationResult.Failure(e);
        }
    }
}