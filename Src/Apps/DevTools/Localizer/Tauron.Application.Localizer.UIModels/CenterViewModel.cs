using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using DynamicData;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Processing.Messages;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using ActorRefFactoryExtensions = Tauron.Akka.ActorRefFactoryExtensions;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class CenterViewModel : UiActor
    {
        public CenterViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, IOperationManager manager,
            LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            IMainWindowCoordinator mainWindow, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            var proxy = Context.ActorOf<ObservableActor>("Loading_Proxy");

            this.Receive<IncommingEvent>(e => e.Action());

            Views = this.RegisterUiCollection<ProjectViewContainer>(nameof(Views)).BindToList(out var viewList);
            CurrentProject = RegisterProperty<int?>(nameof(CurrentProject));

            AddProject(new Project {ProjectName = "Dummy"});

            static string GetActorName(string projectName) => projectName.Replace(' ', '_') + "-View";

            #region Project Save

            void ProjectSaved((SavedProject, OperationController?) input)
            {
                var ((_, isOk, error), controller) = input;

                if (isOk)
                {
                    mainWindow.Saved = true;
                    controller?.Compled();
                }
                else
                {
                    mainWindow.Saved = false;
                    controller?.Failed(error?.Message);
                }
            }

            void SaveRequested(SaveRequest obj)
            {
                if (string.IsNullOrWhiteSpace(obj.ProjectFile.Source)) return;

                var operation = manager.StartOperation(string.Format(localizer.CenterViewSaveProjectOperation,
                    Path.GetFileName(obj.ProjectFile.Source)));
                var file = obj.ProjectFile;

                operation.Select(op => new SaveProject(op.Id, file))
                    .ObserveOnSelf()
                    .ToActor(sp => sp.ProjectFile.Operator);
            }

            workspace.Source.SaveRequest
                .ObserveOnSelf()
                .Subscribe(SaveRequested)
                .DisposeWith(this);

            (from savedProject in Receive<SavedProject>()
                    from controller in manager.Find(savedProject.OperationId)
                    select (savedProject, controller))
                .Subscribe(ProjectSaved)
                .DisposeWith(this);

            #endregion

            #region Update Source

            Receive<UpdateSource>().Mutate(workspace.Source)
                .With(sm => sm.SourceUpdate, sm => us => sm.UpdateSource(us.Name))
                .Subscribe(su => mainWindow.TitlePostfix = Path.GetFileNameWithoutExtension(su.Source));

            #endregion

            #region Remove Project

            RemoveProjectName? TryGetRemoveProjectName()
            {
                var currentProject = CurrentProject.Value;
                if (currentProject == null) return null;

                var (_, projectName, _, _) = Views[currentProject.Value].Project;

                return new RemoveProjectName(projectName);
            }

            void RemoveDialog(RemoveProjectName project)
            {
                UICall(_ =>
                {
                    dialogCoordinator.ShowMessage(
                        string.Format(localizer.CenterViewRemoveProjectDialogTitle, project.Name),
                        localizer.CenterViewRemoveProjectDialogMessage,
                        result =>
                        {
                            if (result == true)
                                workspace.Projects.RemoveProject(project.Name);
                        });
                });
            }

            void RemoveProject(Project project)
            {
                var proj = Views!.FirstOrDefault(p => p.Project.ProjectName == project.ProjectName);
                if (proj == null) return;

                Context.Stop(proj.Model.Actor);
                viewList!.Remove(proj);
            }

            NewCommad.WithCanExecute(from project in CurrentProject
                    from file in workspace
                    select project != null && !file.IsEmpty)
                .WithFlow(TryGetRemoveProjectName,
                    obs => obs.NotNull()
                        .Mutate(workspace.Projects).With(pm => pm.RemovedProject, _ => RemoveDialog)
                        .Subscribe(rp => RemoveProject(rp.Project)))
                .ThenRegister("RemoveProject");

            #endregion

            #region Project Reset

            Start.Subscribe(_ => Self.Tell(new ProjectRest(workspace.ProjectFile)));
            Stop.Subscribe(_ =>
            {
                Views.Foreach(c => Context.Stop(c.Model.Actor));
                Thread.Sleep(1000);
            });

            void ProjectRest(ProjectRest obj)
            {
                mainWindow.Saved = File.Exists(obj.ProjectFile.Source);

                var target = Observable.Return(obj);

                if (Views!.Count != 0)
                    target = target
                        .ObserveOn(ActorScheduler.From(proxy))
                        .SelectMany(pr => Task.WhenAll(Views.Select(c
                                => c.Model.Actor.GracefulStop(TimeSpan.FromMinutes(1))
                                    .ContinueWith(t => (c.Model.Actor, t))))
                            .ContinueWith(t =>
                            {
                                try
                                {
                                    foreach (var (actor, stopped) in t.Result)
                                    {
                                        try
                                        {
                                            if (stopped.Result)
                                                continue;
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error(e, "Error on Stop Project Actor");
                                        }

                                        actor.Tell(Kill.Instance);
                                    }
                                }
                                finally
                                {
                                    viewList!.Clear();
                                }

                                return pr;
                            }));

                target
                    .ObserveOnSelf()
                    .Subscribe(pr =>
                    {
                        string titleName = pr.ProjectFile.Source;
                        if (string.IsNullOrWhiteSpace(titleName))
                        {
                            titleName = localizer.CommonUnkowen;
                        }
                        else
                        {
                            titleName = Path.GetFileNameWithoutExtension(pr.ProjectFile.Source);
                            if (string.IsNullOrWhiteSpace(titleName))
                                titleName = localizer.CommonUnkowen;
                        }

                        mainWindow.TitlePostfix = titleName;

                        foreach (var project in pr.ProjectFile.Projects)
                            AddProject(project);

                        mainWindow.IsBusy = false;
                    });
            }

            Receive<SupplyNewProjectFile>(obs => obs
                .Mutate(workspace.Source).With(sm => sm.ProjectReset, sm => np => sm.Reset(np.File))
                .ObserveOnSelf()
                .Subscribe(ProjectRest));

            #endregion

            #region Add Project

            void AddProject(Project project)
            {
                string name = GetActorName(project.ProjectName);
                if (!ActorPath.IsValidPathElement(name))
                {
                    UICall(_ => dialogCoordinator.ShowMessage(localizer.CommonError,
                        localizer.CenterViewNewProjectInvalidNameMessage));
                    return;
                }

                var view = LifetimeScope.Resolve<IViewModel<ProjectViewModel>>();
                view.InitModel(Context, name);

                view.AwaitInit(() => view.Actor.Tell(new InitProjectViewModel(project), Self));
                viewList.Add(new ProjectViewContainer(project, view));

                UICall(() => CurrentProject += Views.Count - 1);
            }

            NewCommad
                .WithCanExecute(workspace.Select(pf => !pf.IsEmpty))
                .WithFlow(ob => ob.Select(_ => workspace.ProjectFile.Projects.Select(p => p.ProjectName))
                    .Dialog(this).Of<IProjectNameDialog, NewProjectDialogResult>()
                    .Mutate(workspace.Projects).With(pm => pm.NewProject, pm => result => pm.AddProject(result.Name))
                    .Select(p => p.Project)
                    .ObserveOnSelf()
                    .Subscribe(AddProject))
                .ThenRegister("AddNewProject");

            #endregion

            #region Add Global Language

            NewCommad
                .WithCanExecute(workspace.Select(pf => !pf.IsEmpty))
                .WithFlow(obs => obs
                    .Select(_ => workspace.ProjectFile.GlobalLanguages.Select(al => al.ToCulture()))
                    .Dialog(this).Of<ILanguageSelectorDialog, AddLanguageDialogResult?>()
                    .NotNull()
                    .Mutate(workspace.Projects).With(mutator => result => mutator.AddLanguage(result.CultureInfo)))
                .ThenRegister("AddGlobalLang");

            #endregion
        }

        private UICollectionProperty<ProjectViewContainer> Views { get; }

        private UIProperty<int?> CurrentProject { get; set; }

        private sealed record RemoveProjectName(string Name);
    }
}