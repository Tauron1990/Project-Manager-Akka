﻿// The MIT License (MIT)
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture 
// 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using Akkatecture.Commands;
using Akkatecture.Core;
using Akkatecture.Extensions;
using JetBrains.Annotations;

namespace Akkatecture.Aggregates;

[PublicAPI]
public abstract class AggregateManager<TAggregate, TIdentity, TCommand> : ReceiveActor,
    IAggregateManager<TAggregate, TIdentity>
    where TAggregate : ReceivePersistentActor, IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
    where TCommand : ICommand<TAggregate, TIdentity>
{
    protected AggregateManager()
    {
        Logger = Context.GetLogger();
        Settings = new AggregateManagerSettings(Context.System.Settings.Config);
        Name = GetType().PrettyPrint();
        Receive<Terminated>(Terminate);

        if(Settings.AutoDispatchOnReceive)
            Receive<TCommand>(Dispatch);

        if(!Settings.HandleDeadLetters)
            return;

        Context.System.EventStream.Subscribe(Self, typeof(DeadLetter));
        Receive(DeadLetterHandler);
    }

    protected ILoggingAdapter Logger { get; set; }
    protected Func<DeadLetter, bool> DeadLetterHandler => Handle;
    public AggregateManagerSettings Settings { get; }
    public string Name { get; }

    protected virtual bool Dispatch(TCommand command)
    {
        Logger.Info(
            "AggregateManager of Type={0}; has received a command of Type={1}",
            Name,
            command.GetType().PrettyPrint());

        IActorRef aggregateRef = FindOrCreate(command.AggregateId);

        aggregateRef.Forward(command);

        return true;
    }


    protected virtual bool ReDispatch(TCommand command)
    {
        Logger.Info(
            "AggregateManager of Type={0}; is ReDispatching deadletter of Type={1}",
            Name,
            command.GetType().PrettyPrint());

        IActorRef aggregateRef = FindOrCreate(command.AggregateId);

        aggregateRef.Forward(command);

        return true;
    }

    protected bool Handle(DeadLetter deadLetter)
    {
        if(deadLetter.Message is not TCommand command ||
           command.GetPropertyValue("AggregateId")?.GetType() != typeof(TIdentity)) return true;

        return ReDispatch(command);
    }

    protected virtual bool Terminate(Terminated message)
    {
        Logger.Warning(
            "Aggregate of Type={0}, and Id={1}; has terminated.",
            typeof(TAggregate).PrettyPrint(),
            message.ActorRef.Path.Name);
        Context.Unwatch(message.ActorRef);

        return true;
    }

    protected virtual IActorRef FindOrCreate(TIdentity aggregateId)
    {
        IActorRef aggregate = Context.Child(aggregateId);

        if(aggregate.IsNobody()) aggregate = CreateAggregate(aggregateId);

        return aggregate;
    }

    protected virtual IActorRef CreateAggregate(TIdentity aggregateId)
    {
        IActorRef? aggregateRef = Context.ActorOf(Props.Create<TAggregate>(args: aggregateId), aggregateId.Value);
        Context.Watch(aggregateRef);

        return aggregateRef;
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        ILoggingAdapter logger = Logger;

        return new OneForOneStrategy(
            3,
            3000,
            exception =>
            {
                logger.Warning(
                    "AggregateManager of Type={0}; will supervise Exception={1} to be decided as {2}.",
                    Name,
                    exception.ToString(),
                    Directive.Restart);

                return Directive.Restart;
            });
    }
}