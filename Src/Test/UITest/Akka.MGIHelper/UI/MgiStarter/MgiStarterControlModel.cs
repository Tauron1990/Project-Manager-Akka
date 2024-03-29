﻿using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.ProcessManager;
using JetBrains.Annotations;
using Tauron;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Application.Wpf;
using Tauron.Features;
using Tauron.Localization;

namespace Akka.MGIHelper.UI.MgiStarter
{
    [UsedImplicitly]
    public sealed class MgiStarterControlModel : UiActor
    {
        private readonly ProcessConfig _config;
        private readonly LocalHelper _localHelper;
        private readonly IActorRef _processManager;

#pragma warning disable MA0051
        public MgiStarterControlModel(IServiceProvider lifetimeScope, IUIDispatcher dispatcher, ProcessConfig config, IDialogFactory dialogFactory)
#pragma warning restore MA0051
            : base(lifetimeScope, dispatcher)
        {
            Client = RegisterProperty<Process?>(nameof(Client)).OnChange(UpdateLabel);
            Kernel = RegisterProperty<Process?>(nameof(Kernel)).OnChange(UpdateLabel);
            Status = RegisterProperty<string?>(nameof(Status)).OnChange(UpdateLabel);
            InternalStart = RegisterProperty<bool>(nameof(InternalStart)).OnChange(UpdateLabel);
            StatusLabel = RegisterProperty<string?>(nameof(StatusLabel));

            _localHelper = new LocalHelper(Context);
            _config = config;

            _processManager = Context.ActorOf("Process-Manager", ProcessManagerActor.New());
            var mgiStarting = Context.ActorOf("Mgi-Starter", MgiStartingActor.New(dialogFactory, _processManager));

            var currentStart = new BehaviorSubject<CancellationTokenSource?>(value: null).DisposeWith(this);
            (from start in currentStart
             from state in InternalStart
             where !state
             select start
                ).SubscribeWithStatus(s => s?.Dispose())
               .DisposeWith(this);

            Receive<ProcessStateChange>(obs => obs.SubscribeWithStatus(ProcessStateChangeHandler));
            Receive<MgiStartingActor.TryStartResponse>(
                obs => obs.SubscribeWithStatus(
                    _ =>
                    {
                        currentStart.OnNext(value: null);
                        InternalStart += false;
                    }));
            Receive<MgiStartingActor.StartStatusUpdate>(obs => obs.SubscribeWithStatus(s => Status += s.Status));

            NewCommad
               .WithCanExecute(InternalStart.Select(b => !b))
               .WithExecute(
                    () =>
                    {
                        InternalStart += true;
                        currentStart.OnNext(new CancellationTokenSource());

                        mgiStarting.Tell(
                            new MgiStartingActor.TryStart(
                                config,
                                currentStart.Value!.Token,
                                () =>
                                {
                                    Client.Value?.Kill(entireProcessTree: true);
                                    Kernel.Value?.Kill(entireProcessTree: true);
                                }));
                    }).ThenRegister("TryStart");

            NewCommad
               .WithCanExecute(Kernel.CombineLatest(Client, (kernel, client) => kernel is not null || client is not null))
               .WithExecute(
                    () =>
                    {
                        currentStart.Value?.Cancel();
                        Client.Value?.Kill(entireProcessTree: true);
                        Kernel.Value?.Kill(entireProcessTree: true);
                    }).ThenRegister("TryStop");

            InternalStart += false;
            UpdateLabel();
        }

        private UIProperty<Process?> Client { get; set; }

        private UIProperty<Process?> Kernel { get; set; }

        private UIProperty<string?> Status { get; set; }

        private UIProperty<bool> InternalStart { get; set; }

        private UIProperty<string?> StatusLabel { get; set; }

        // ReSharper disable once CognitiveComplexity
        private void ProcessStateChangeHandler(ProcessStateChange obj)
        {
            try
            {
                var processChange = obj.Change;
                var name = obj.Name;
                var process = obj.Process;
                switch (processChange)
                {
                    case ProcessChange.Started:
                        if (_config.Kernel.Contains(name, StringComparison.Ordinal))
                        {
                            //ConfigProcess(process);
                            Kernel += process;
                        }

                        if (_config.Client.Contains(name, StringComparison.Ordinal))
                        {
                            //ConfigProcess(process);
                            Client += process;
                        }

                        break;
                    case ProcessChange.Stopped:
                        if (_config.Kernel.Contains(name, StringComparison.Ordinal))
                            Kernel += null!;
                        if (_config.Client.Contains(name, StringComparison.Ordinal))
                            Client += null!;

                        break;
                    default:
                        #pragma warning disable EX006
                        throw new InvalidOperationException("Invalid ProcessChange Enum");
                    #pragma warning restore EX006
                }
                
                // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (Kernel is null && Client is null)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    Status += Context.Loc().RequestString("uistatusstartet").Value;
                if (Kernel!.Value is null && Client!.Value is null)
                    Status += Context.Loc().RequestString("uistatusstopped").Value;
            }
            catch (Exception e)
            {
                #pragma warning disable EPC12
                Status += "Fehler: " + e.Message;
                #pragma warning restore EPC12
            }
        }

        protected override void Initialize(InitEvent evt)
            => _processManager.Tell(
                new RegisterProcessList(
                    Self,
                    new StartProcessTracking(
                        ImmutableList<string>.Empty.Add(_config.Client).Add(_config.Kernel),
                        _config.ClientAffinity,
                        _config.OperatingSystemAffinity,
                        _config.ClientProcesses)));


        private void UpdateLabel()
        {
            var builder = new StringBuilder();

            var status = Status;
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            bool kernel = Kernel is null;
            bool client = Client is null;
            // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (!string.IsNullOrWhiteSpace(status) && status.Value?.StartsWith("Fehler:", StringComparison.Ordinal) == true)
            {
                StatusLabel = status;

                return;
            }

            builder.AppendLine(string.IsNullOrWhiteSpace(status) ? _localHelper.Unkowen : status);

            builder.Append("Kernel: ");
            builder.AppendLine(kernel ? _localHelper.GenericStart : _localHelper.GenericNotStart);
            builder.Append("Client: ");
            builder.AppendLine(client ? _localHelper.GenericStart : _localHelper.GenericNotStart);

            StatusLabel += builder.ToString();
        }

        private class LocalHelper
        {
            internal LocalHelper(IActorContext context)
            {
                var loc = context.Loc();

                Unkowen = loc.RequestString("genericunkowen").Value;
                GenericStart = loc.RequestString("genericstart").Value;
                GenericNotStart = loc.RequestString("genericnotstart").Value;
            }

            internal string Unkowen { get; }

            internal string GenericStart { get; }

            internal string GenericNotStart { get; }
        }
    }
}