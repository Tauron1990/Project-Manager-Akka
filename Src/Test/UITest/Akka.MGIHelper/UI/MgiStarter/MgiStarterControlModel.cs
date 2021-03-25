using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Akka.Actor;
using Akka.DI.Core;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.ProcessManager;
using Autofac;
using Tauron;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Application.Wpf;
using Tauron.Features;
using Tauron.Localization;

namespace Akka.MGIHelper.UI.MgiStarter
{
    public sealed class MgiStarterControlModel : UiActor
    {
        private readonly ProcessConfig _config;
        private readonly LocalHelper _localHelper;
        private readonly IActorRef _processManager;

        public MgiStarterControlModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, ProcessConfig config, IDialogFactory dialogFactory)
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
            var mgiStarting = Context.ActorOf("Mgi-Starter", MgiStartingActor.New(dialogFactory));

            var currentStart = new BehaviorSubject<CancellationTokenSource?>(null).DisposeWith(this);
            (from start in currentStart
             from state in InternalStart
             where !state
             select start
                ).SubscribeWithStatus(s => s?.Dispose())
                 .DisposeWith(this);

            Receive<ProcessStateChange>(obs => obs.SubscribeWithStatus(ProcessStateChangeHandler));
            Receive<MgiStartingActor.TryStartResponse>(obs => obs.SubscribeWithStatus(_ => InternalStart += false));
            Receive<MgiStartingActor.StartStatusUpdate>(obs => obs.SubscribeWithStatus(s => Status += s.Status));

            NewCommad
               .WithCanExecute(InternalStart.Select(b => !b))
               .WithExecute(() =>
                            {
                                InternalStart += true;
                                currentStart.OnNext(new CancellationTokenSource());

                                mgiStarting.Tell(new MgiStartingActor.TryStart(_config, currentStart.Value!.Token,
                                    () =>
                                    {
                                        Client.Value?.Kill(true);
                                        Kernel.Value?.Kill(true);
                                    }));
                            }).ThenRegister("TryStart");

            NewCommad
               .WithCanExecute(from client in Client
                               from kernel in Kernel 
                               select client != null || kernel != null)
               .WithExecute(() =>
                            {
                                currentStart.Value?.Cancel();
                                Client.Value?.Kill(true);
                                Kernel.Value?.Kill(true);
                            }).ThenRegister("TryStop");

            InternalStart += false;
            UpdateLabel();
        }

        private UIProperty<Process?> Client { get; set; }

        private UIProperty<Process?> Kernel { get; set; }

        private UIProperty<string?> Status { get; set; }

        private UIProperty<bool> InternalStart { get; set; }

        private UIProperty<string?> StatusLabel { get; set; }

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
                        if (_config.Kernel.Contains(name))
                        {
                            ConfigProcess(process);
                            Kernel += process;
                        }

                        if (_config.Client.Contains(name))
                        {
                            ConfigProcess(process);
                            Client += process;
                        }

                        break;
                    case ProcessChange.Stopped:
                        if (_config.Kernel.Contains(name))
                            Kernel += null!;
                        if (_config.Client.Contains(name))
                            Client += null!;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid ProcessChange Enum");
                }

                if (Kernel != null && Client != null)
                    Status += Context.Loc().RequestString("uistatusstartet");
                if (Kernel!.Value == null && Client!.Value == null)
                    Status += Context.Loc().RequestString("uistatusstopped");
            }
            catch (Exception e)
            {
                Status += "Fehler: " + e.Message;
            }
        }

        protected override void Initialize(InitEvent evt)
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            _processManager.Tell(new RegisterProcessList(Self, ImmutableArray<string>.Empty.Add(_config.Client).Add(_config.Kernel)));
        }

        private static void ConfigProcess(Process p)
        {
            if (p.PriorityClass != ProcessPriorityClass.High)
                p.PriorityClass = ProcessPriorityClass.High;
        }


        private void UpdateLabel()
        {
            var builder = new StringBuilder();
            
            var status = Status;
            var kernel = Kernel != null;
            var client = Client != null;
            if (!string.IsNullOrWhiteSpace(status) && status.Value?.StartsWith("Fehler:") == true)
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
            public LocalHelper(IActorContext context)
            {
                var loc = context.Loc();

                Unkowen = loc.RequestString("genericunkowen");
                GenericStart = loc.RequestString("genericstart");
                GenericNotStart = loc.RequestString("genericnotstart");
            }

            public string Unkowen { get; }

            public string GenericStart { get; }

            public string GenericNotStart { get; }
        }
    }
}