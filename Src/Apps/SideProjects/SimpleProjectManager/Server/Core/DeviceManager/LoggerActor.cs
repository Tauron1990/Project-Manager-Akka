﻿using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DependencyInjection;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public sealed class LoggerActor : ReceiveActor
{
    private readonly SortedDictionary<DateTime, LogBatch> _batches = new();
    private readonly DeviceId _deviceName;
    private readonly DeviceEventHandler _handler;

    private readonly LogDistribution _logDistribution;

    public LoggerActor(DeviceId deviceName, DeviceEventHandler handler)
    {
        _deviceName = deviceName;
        _handler = handler;
        _logDistribution = new LogDistribution(Context.System);
        Receive<SubscribeAck>(_ => Become(Ready));
    }

    public static Props Create(ActorSystem system, DeviceId deviceName)
        => DependencyResolver.For(system).Props<LoggerActor>(deviceName);

    private void Ready()
    {
        Receive<LogBatch>(NewBatch);
        Receive<DeviceManagerMessages.QueryLoggerBatch>(QueryBatch);
    }

    private void QueryBatch(DeviceManagerMessages.QueryLoggerBatch obj)
    {
        Sender.Tell(
            new DeviceManagerMessages.LoggerBatchResult(
                _batches
                   .Where(p => p.Key > obj.From && p.Key <= obj.To)
                   .Select(p => p.Value)
                   .ToImmutableList()));
    }

    private void NewBatch(LogBatch obj)
    {
        if(obj.DeviceName != _deviceName) return;

        DateTime newKey = DateTime.UtcNow;
        _batches.Add(newKey, obj);

        var counter = 0;
        var toDelete = _batches
           .Reverse()
           .SkipWhile(
                p =>
                {
                    counter += p.Value.Logs.Count;

                    return counter < 1000;
                })
           .Select(p => p.Key)
           .ToArray();

        foreach (var key in toDelete)
            _batches.Remove(key);

        _handler.Publish(new NewBatchesArrived(obj.DeviceName, newKey));
    }

    protected override void PreStart()
    {
        _logDistribution.Subscribe(Self);
        base.PreStart();
    }
}