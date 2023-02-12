using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer.Impl.Source;
using Tauron;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    [UsedImplicitly]
    public sealed class ActualInstallerActor : LambdaWorkflowActor<InstallerContext>
    {
        private static readonly StepId Preperation = new(nameof(Preperation));
        private static readonly StepId Validation = new(nameof(Validation));
        private static readonly StepId PreCopy = new(nameof(PreCopy));
        private static readonly StepId Copy = new(nameof(Copy));
        private static readonly StepId Registration = new(nameof(Registration));
        private static readonly StepId Finalization = new(nameof(Finalization));

        public ActualInstallerActor(IAppRegistry registry, AppNodeInfo configuration, IAutoUpdater autoUpdater)
        {
            StartMessage<FileInstallationRequest>(HandleFileInstall);

            WhenStep(StepId.Start, c => c.OnExecute(_ => Preperation));

            WhenStep(
                Preperation,
                config =>
                {
                    config.OnExecute(
                        (context, step) =>
                        {
                            Log.Info("Perpering Data for Installation: {Apps}", context.Name);
                            switch (context.SetSource(InstallationSourceSelector.Select, step.SetError))
                            {
                                case EmptySource:
                                    return StepId.Fail;
                                default:
                                    registry.Ask<InstalledAppRespond>(new InstalledAppQuery(context.Name), TimeSpan.FromSeconds(10))
                                       .PipeTo(Self).Ignore();

                                    return StepId.Waiting;
                            }
                        });

                    Signal<InstalledAppRespond>(
                        (context, respond) =>
                        {
                            var (installedApp, fault) = respond;
                            if (!fault)
                            {
                                context.SetInstalledApp(installedApp);

                                return Validation;
                            }

                            SetError(ErrorCodes.QueryAppInfo);

                            return StepId.Fail;
                        });
                });

            WhenStep(
                Validation,
                confg =>
                {
                    confg.OnExecute(
                        (context, step) =>
                        {
                            Log.Info("Validating Data for installation: {Apps}", context.Name);
                            if (context.Source.ValidateInput(context) is Status.Failure failure)
                            {
                                Log.Warning(failure.Cause, "Source Validation Failed {Apps}", context.Name);
                                step.ErrorMessage = failure.Cause.Message;

                                return StepId.Fail;
                            }

                            // ReSharper disable once InvertIf
                            if (!context.InstalledApp.IsEmpty() && context.InstalledApp.Name == context.Name && !context.Override)
                            {
                                Log.Warning("Apps is Installed {Apps}", context.Name);
                                step.ErrorMessage = ErrorCodes.ExistingApp;

                                return StepId.Fail;
                            }

                            return PreCopy;
                        });
                });

            WhenStep(
                PreCopy,
                config =>
                {
                    config.OnExecute(
                        (context, step) =>
                        {
                            Log.Info("Prepare for Copy Data {Apps}", context.Name);
                            string targetAppPath = Path.GetFullPath(Path.Combine(configuration.AppsLocation, context.Name.Value));


                            if (context.AppType != AppType.Host)
                                try
                                {
                                    if (!Directory.Exists(targetAppPath))
                                        Directory.CreateDirectory(targetAppPath);
                                }
                                catch (Exception e)
                                {
                                    Log.Warning(e, "Installation Faild during Directory Creation {Apps}", context.Name);
                                    step.ErrorMessage = ErrorCodes.DirectoryCreation;

                                    return StepId.Fail;
                                }

                            context.InstallationPath = targetAppPath;
                            context.Source.PrepareforCopy(context)
                               .PipeTo(Self, success: () => new PreCopyCompled());


                            if (context.AppType != AppType.Host)
                                if (context.Override)
                                {
                                    context.Backup.Make(targetAppPath);
                                    context.Recovery.Add(context.Backup.Recover);
                                }

                            return StepId.Waiting;
                        });

                    Signal<PreCopyCompled>((_, _) => Copy);
                });

            WhenStep(
                Copy,
                config =>
                {
                    config.OnExecute(
                        (context, step) =>
                        {
                            Log.Info("Copy Application Data {Apps}", context.Name);


                            if (context.AppType == AppType.Host)
                            {
                                autoUpdater.Tell(new StartAutoUpdate(context.Source.ToZipFile(context)));

                                return StepId.Finish;
                            }

                            context.Recovery.Add(
                                log =>
                                {
                                    log.Info("Clearing Installation Directory during Recover {Apps}", context.Name);
                                    ClearDirectory(context.InstallationPath);
                                });

                            try
                            {
                                context.Source.CopyTo(context, context.InstallationPath)
                                   .PipeTo(Self, success: () => new CopyCompled());
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, "Error on Extracting Files to Directory {Apps}", context.Name);
                                step.ErrorMessage = e.Message;

                                return StepId.Fail;
                            }

                            context.Recovery.Add(
                                log =>
                                {
                                    log.Info("Delete Insttalation Files during Recovery {Apps}", context.Name);
                                    ClearDirectory(context.InstallationPath);
                                });

                            return StepId.Waiting;
                        });

                    Signal<CopyCompled>((_, _) => Registration);
                });

            WhenStep(
                Registration,
                config =>
                {
                    config.OnExecute(
                        (context, _) =>
                        {
                            Log.Info("Register Application for Host {Apps}", context.Name);

                            if (context.InstalledApp.IsEmpty())
                                registry.Ask<RegistrationResponse>(
                                        new NewRegistrationRequest(context.SoftwareName, context.Name, context.InstallationPath, context.Source.Version, context.AppType, context.GetExe()),
                                        TimeSpan.FromSeconds(15))
                                   .PipeTo(Self).Ignore();
                            else
                                registry
                                   .Ask<RegistrationResponse>(new UpdateRegistrationRequest(context.Name), TimeSpan.FromSeconds(15))
                                   .PipeTo(Self).Ignore();

                            return StepId.Waiting;
                        });

                    Signal<RegistrationResponse>(
                        (_, response) =>
                        {
                            var (scceeded, exception) = response;

                            if (scceeded)
                                return Finalization;

                            SetError(exception?.Message ?? "");

                            return StepId.Fail;
                        });
                });

            WhenStep(
                Finalization,
                config =>
                {
                    config.OnExecute(
                        context =>
                        {
                            Log.Info("Clean Up and Compleding {Apps}", context.Name);

                            context.Backup.CleanUp();

                            try
                            {
                                context.Source.CleanUp(context);
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e, "Error on Clean Up {Apps}", context.Name);
                            }

                            return StepId.Finish;
                        });
                });

            Signal<Status.Failure>(
                (_, f) =>
                {
                    SetError(f.Cause.Message);

                    return StepId.Fail;
                });

            OnFinish(
                wr =>
                {
                    var (succesfully, error, installerContext) = wr;
                    if (!succesfully)
                    {
                        Log.Warning("Installation Failed Recover {Apps}", installerContext.Name);
                        installerContext.Recovery.Recover(Log);
                    }

                    var finish = new InstallerationCompled(succesfully, error, installerContext.AppType, installerContext.Name, InstallationAction.Install);
                    if (!Sender.Equals(Context.System.DeadLetters))
                        Sender.Tell(finish, ActorRefs.NoSender);

                    Context.Parent.Tell(finish);
                    Context.Stop(Self);
                });
        }

        private void HandleFileInstall(FileInstallationRequest request)
            => Start(new InstallerContext(InstallType.Manual, request.Name, request.SoftwareName, request.Path, request.Override, request.AppType) { Exe = request.Exe });

        private void ClearDirectory(string path)
        {
            foreach (FileSystemInfo info in new DirectoryInfo(path).EnumerateFileSystemInfos())
            {
                switch (info)
                {
                    case DirectoryInfo subDic:
                        subDic.Delete(recursive: true);
                        break;
                    case FileInfo file:
                        file.Delete();
                        break;
                }
            }
        }
        
        private sealed class PreCopyCompled { }

        private sealed class CopyCompled { }
    }
}