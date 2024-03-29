﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using UnitsNet;

using ReporterResult = FluentResults.Result<Tauron.Application.AkkaNode.Services.Reporting.Reporter>;
using Result = FluentResults.Result;

namespace Tauron.Application.AkkaNode.Services.Reporting;

[PublicAPI]
public sealed class Reporter
{
    public const string TimeoutError = nameof(TimeoutError);
    public static readonly Reporter Empty = new(ActorRefs.Nobody);
    private readonly AtomicBoolean _compledCalled = new();

    private readonly IActorRef _reporter;

    public Reporter(IActorRef reporter) => _reporter = reporter;

    public bool IsCompled => _compledCalled.Value;

    public static Reporter CreateReporter(IActorRefFactory factory, string? name = null)
        => new(factory.ActorOf(Props.Create(() => new ReporterActor()).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name));

    public static IActorRef CreateListner(
            IActorRefFactory factory, Action<string> listner,
            #pragma warning disable EPS05
            Action<IOperationResult> onCompled, Duration? timeout, string? name = null)
        #pragma warning restore EPS05
        => factory.ActorOf(
            Props.Create(() => new Listner(listner, onCompled, timeout))
               .WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy),
            name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner,
        Action<IOperationResult> onCompled, string? name = null)
        => CreateListner(factory, listner, onCompled, timeout: null, name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Reporter reporter,
        Action<IOperationResult> onCompled, in Duration? timeout, string? name = null)
        => CreateListner(factory, reporter.Send, onCompled, timeout, name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner,
        TaskCompletionSource<IOperationResult> onCompled, in Duration? timeout, string? name = null)
        => CreateListner(factory, listner, onCompled.SetResult, timeout, name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner,
        TaskCompletionSource<IOperationResult> onCompled, string? name = null)
        => CreateListner(factory, listner, onCompled: onCompled, timeout: null, name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Reporter reporter,
        TaskCompletionSource<IOperationResult> onCompled, in Duration? timeout, string? name = null)
        => CreateListner(factory, reporter.Send, onCompled, timeout, name);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner, in Duration? timeout,
        string? name, Action<Task<IOperationResult>> onCompled)
    {
        var source = new TaskCompletionSource<IOperationResult>();
        IActorRef actor = CreateListner(factory, listner, source, timeout, name);
        onCompled(source.Task);

        return actor;
    }

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner, string name,
        Action<Task<IOperationResult>> onCompled)
        => CreateListner(factory, listner, timeout: null, name, onCompled);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Reporter reporter, in Duration? timeout,
        string? name, Action<Task<IOperationResult>> onCompled)
        => CreateListner(factory, reporter.Send, timeout, name, onCompled);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner, in Duration? timeout,
        Action<Task<IOperationResult>> onCompled)
        => CreateListner(factory, listner, timeout, name: null, onCompled);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner,
        Action<Task<IOperationResult>> onCompled)
        => CreateListner(factory, listner, timeout: null, name: null, onCompled);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Reporter reporter, in Duration? timeout,
        Action<Task<IOperationResult>> onCompled)
        => CreateListner(factory, reporter.Send, timeout, name: null, onCompled);

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner, in Duration? timeout,
        string? name, out Task<IOperationResult> onCompled)
    {
        var source = new TaskCompletionSource<IOperationResult>();
        IActorRef actor = CreateListner(factory, listner, source, timeout, name);
        onCompled = source.Task;

        return actor;
    }

    public static IActorRef CreateListner(
        IActorRefFactory factory, Action<string> listner, in Duration? timeout,
        out Task<IOperationResult> onCompled)
        => CreateListner(factory, listner, timeout, name: null, out onCompled);

    public ReporterResult Listen(IActorRef actor)
    {
        if(_reporter.IsNobody()) return this;

        if(_compledCalled.Value)
            return Result.Fail(new ReporterCompledError());

        _reporter.Tell(new ListeningActor(actor));

        return this;
    }

    public Result Send(string message)
    {
        if(_reporter.IsNobody()) return Result.Fail(new NoReporterError());

        if(_compledCalled.Value)
            return Result.Fail(new ReporterCompledError());

        _reporter.Tell(new TransferedMessage(message));
    }

    public void Send<T>(T message)
        where T : notnull
        => Send(message.ToString() ?? string.Empty);
    
    public Result Compled(IOperationResult result)
    {
        if(_reporter.IsNobody()) return Result.Fail(new NoReporterError());

        if(_compledCalled.GetAndSet(newValue: true))
            return Result.Fail(new ReporterCompledError());

        _reporter.Tell(result);
        
        return Result.Ok();
    }

    private sealed class Listner : ReceiveActor
    {
        private bool _compled;

        #pragma warning disable GU0073
        public Listner(Action<string> listner, Action<IOperationResult> onCompled, Duration? timeSpan)
            #pragma warning restore GU0073
        {
            Receive<IOperationResult>(
                c =>
                {
                    if(_compled) return;

                    _compled = true;
                    Context.Stop(Self);
                    onCompled(c);
                });
            Receive<TransferedMessage>(m => listner(m.Message));

            if(timeSpan is null)
                return;

            Task.Delay(timeSpan.Value.ToTimeSpan()).PipeTo(Self, success: () => OperationResult.Failure(new Error(TimeoutError, TimeoutError)));
            //Timers.StartSingleTimer(timeSpan, OperationResult.Failure(new Error(TimeoutError, TimeoutError)), timeSpan);
        }
    }

    private sealed class ReporterActor : ReceiveActor
    {
        private readonly List<IActorRef> _listner = new();

        #pragma warning disable GU0073
        public ReporterActor()
            #pragma warning restore GU0073
        {
            Receive<TransferedMessage>(
                msg =>
                {
                    foreach (IActorRef actorRef in _listner) actorRef.Forward(msg);
                });

            Receive<IOperationResult>(
                msg =>
                {
                    foreach (IActorRef actorRef in _listner) actorRef.Forward(msg);
                    Context.Stop(Self);
                });

            Receive<ListeningActor>(
                a =>
                {
                    Context.Watch(a.Actor);
                    _listner.Add(a.Actor);
                });

            Receive<Terminated>(t => { _listner.Remove(t.ActorRef); });
        }
    }

    private sealed record ListeningActor(IActorRef Actor);

    private sealed record TransferedMessage(string Message);
}