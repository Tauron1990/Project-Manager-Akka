﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Actor;
using Akkatecture.Commands;
using Akkatecture.Core;

namespace SimpleProjectManager.Server.Core;

public class CommandProcessor
{
    private readonly ImmutableDictionary<Type, ApiCommandMapping> _apiCommandMappings;
    private readonly CommandMapping[] _commandMappings;
    private readonly ILogger<CommandProcessor> _log;
    private readonly ConcurrentDictionary<string, IActorRef> _managers = new(StringComparer.Ordinal);
    private readonly Guid _nameNamespace = Guid.Parse("587C4D9F-04D2-4180-BD73-544338280714");

    private readonly ActorSystem _system;

    public CommandProcessor(ActorSystem system, IEnumerable<CommandMapping> mapping, IEnumerable<ApiCommandMapping> apiCommandMappings, ILogger<CommandProcessor> log)
    {
        _system = system;
        _log = log;
        _commandMappings = mapping.ToArray();
        _apiCommandMappings = apiCommandMappings.ToImmutableDictionary(m => m.TargetType);
    }

    private async Task<IOperationResult> RunCommand(ICommand command, CancellationToken token)
    {
        CommandMapping? handler = _commandMappings.FirstOrDefault(cm => cm.CommandType.IsInstanceOfType(command));

        if(handler is null) return OperationResult.Failure(new Error("Kein Handler gefunden", "No-Handler"));

        var name = GuidFactories.Deterministic.Create(_nameNamespace, handler.AggregateManager.AssemblyQualifiedName!).ToString("N");
        IActorRef manager = _managers.GetOrAdd(name, static (n, p) => p.System.ActorOf(Props.Create(p.Handler.AggregateManager), n), (System: _system, Handler: handler));

        try
        {
            return await manager.Ask<IOperationResult>(command, TimeSpan.FromSeconds(30), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Failure(new Error("Kommando hat zu lange gebraucht", "cmmand-timeout"));
        }
    }

    public async Task<IOperationResult> RunCommand(object apiCommand, CancellationToken token)
    {
        try
        {
            if(_apiCommandMappings.TryGetValue(apiCommand.GetType(), out ApiCommandMapping? mapping))
                return await RunCommand(mapping.Converter(apiCommand), token).ConfigureAwait(false);

            return OperationResult.Failure("Kommando Konverter nicht gefunden", "no-converter");
        }
        catch (Exception e)
        {
            _log.LogError(e, "Error on Process Commando {Command}", apiCommand);

            return OperationResult.Failure(e);
        }
    }
}