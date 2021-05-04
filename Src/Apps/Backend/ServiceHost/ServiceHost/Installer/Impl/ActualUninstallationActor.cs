using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Services;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    [UsedImplicitly]
    public sealed class ActualUninstallationActor : LambdaWorkflowActor<UnistallContext>
    {
        private static readonly StepId Stopping = new(nameof(Stopping));
        private static readonly StepId Unistall = new(nameof(Unistall));
        private static readonly StepId Finalization = new(nameof(Finalization));

        public ActualUninstallationActor(IAppRegistry registry, IAppManager manager)
        {
            WhenStep(StepId.Start, config =>
            {
                config.OnExecute(context =>
                {
                    Log.Info("Start Unistall Apps {Name}", context.Name);
                    registry.Ask<InstalledAppRespond>(new InstalledAppQuery(context.Name), TimeSpan.FromSeconds(15))
                       .PipeTo(Self);

                    return StepId.Waiting;
                });

                Signal<InstalledAppRespond>((context, respond) =>
                {
                    var (installedApp, fault) = respond;
                    if (fault || installedApp.IsEmpty())
                    {
                        Log.Warning("Error on Query Application Info {Name}", context.Name);
                        SetError(ErrorCodes.QueryAppInfo);
                        return StepId.Fail;
                    }

                    context.App = installedApp;
                    return Stopping;
                });
            });

            WhenStep(Stopping, config =>
            {
                config.OnExecute(context =>
                {
                    Log.Info("Stoping Appliocation {Name}", context.Name);
                    manager.Actor
                       .Ask<StopResponse>(new StopApp(context.Name), TimeSpan.FromMinutes(1))
                       .PipeTo(Self);
                    return StepId.Waiting;
                });

                Signal<StopResponse>((_, _) => Unistall);
            });

            WhenStep(Unistall, config =>
            {
                config.OnExecute((context, step) =>
                {
                    try
                    {
                        Log.Info("Backup Application {Name}", context.Name);
                        context.Backup.Make(context.App.Path);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error while making Backup");
                        step.ErrorMessage = e.Message;
                        return StepId.Fail;
                    }

                    context.Recovery.Add(context.Backup.Recover);

                    try
                    {
                        Log.Info("Delete Application Directory {Name}", context.Name);
                        Directory.Delete(context.App.Path, true);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Error while Deleting Apps Directory");
                    }

                    return Finalization;
                });
            });

            Signal<Failure>((_, f) =>
            {
                SetError(f.Exception.Message);
                return StepId.Fail;
            });

            OnFinish(wr =>
            {
                var (succesfully, error, unistallContext) = wr;
                if (!succesfully)
                {
                    Log.Warning("Installation Failed Recover {Apps}", unistallContext.Name);
                    unistallContext.Recovery.Recover(Log);
                }

                var finish = new InstallerationCompled(succesfully, error, unistallContext.App.AppType, unistallContext.Name, InstallationAction.Uninstall);
                if (!Sender.Equals(Context.System.DeadLetters))
                    Sender.Tell(finish, ActorRefs.NoSender);

                Context.Parent.Tell(finish);
                Context.Stop(Self);
            });
        }
    }
}