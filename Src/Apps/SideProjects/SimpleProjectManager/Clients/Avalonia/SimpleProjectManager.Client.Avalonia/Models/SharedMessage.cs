using System;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models;

public enum MessageType
{
    Info,
    Warning,
    Error, 
    Sucess,
}

public sealed record SharedMessage(string Message, MessageType MessageType)
{
    public static SharedMessage CreateError(string error) => new(error, MessageType.Error);
    
    public static SharedMessage CreateError(Exception error) => new(error.Message, MessageType.Error);

    public static SharedMessage CreateInfo(string info) => new(info, MessageType.Info);

    public static SharedMessage CreateWarning(string warning) => new(warning, MessageType.Warning);

    public static SharedMessage CreateSucess(string message) => new(message, MessageType.Sucess);
}

public sealed class SharedMessageEvent : AggregateEvent<SharedMessage>{}

public static class SharedMessageEventExtensions
{
    public static void PublishSharedMessage(this IEventAggregator aggregator, SharedMessage message)
        => aggregator.GetEvent<SharedMessageEvent, SharedMessage>().Publish(message);

    public static IObservable<SharedMessage> ConsumeSharedMessage(this IEventAggregator aggregator)
        => aggregator.GetEvent<SharedMessageEvent, SharedMessage>().Get();
}