using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using DynamicData;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing.Messages;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf;
using Tauron.ObservableExt;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed record ProjectBuildpathRequest(string TargetPath, string Project);

    [PublicAPI]
    public sealed class BuildProjectViewModel : ObservableObject
    {
        private readonly IDialogFactory _dialogFactory;
        private readonly LocLocalizer _localizer;
        private readonly IObserver<ProjectBuildpathRequest> _updatePath;
        private string _path;
        private string _source;

        public BuildProjectViewModel(IObserver<ProjectBuildpathRequest> updatePath, string? path, string project,
            LocLocalizer localizer, IDialogFactory dialogFactory, string? source)
        {
            _path = path ?? string.Empty;
            Project = project;
            _localizer = localizer;
            _dialogFactory = dialogFactory;

            source = source?.GetDirectoryName();
            _source = string.IsNullOrWhiteSpace(source) ? Environment.CurrentDirectory : source;
            _updatePath = updatePath;

            Label = string.Format(localizer.MainWindowBuildProjectLabel, project);
            Search = new SimpleCommand(SetPathDialog);
        }

        public string Project { get; }

        public ICommand Search { get; }

        public string Label { get; }

        public string Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                OnPropertyChanged();

                _updatePath.OnNext(new ProjectBuildpathRequest(value, Project));
            }
        }

        private void SetPathDialog()
        {
            var path = _dialogFactory.ShowOpenFolderDialog(null,
                    _localizer.MainWindowBuildProjectFolderBrowserDescription, _source,
                    showNewFolderButton: true, useDescriptionForTitle: false)
                .NotEmpty()
                .Select(s => s.CombinePath("lang"))
                .Select(s => BuildViewModel.MakeRelativePath(s, _source));

            path.ToTask().ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && !string.IsNullOrWhiteSpace(t.Result))
                    Path = t.Result;
            });
        }

        public void UpdateSource(string newSource)
        {
            var compledPath = _path.CombinePath(_source);
            var newPath = BuildViewModel.MakeRelativePath(compledPath, newSource);
            _source = newSource;
            Path = newPath;
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public sealed class BuildViewModel : UiActor
    {
        public BuildViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, ProjectFileWorkspace workspace,
            LocLocalizer localizer, IDialogFactory dialogFactory, IOperationManager manager)
            : base(lifetimeScope, dispatcher)
        {
            this.Receive<IncommingEvent>(e => e.Action());

            #region Import Integration

            Importintegration = RegisterProperty<bool>(nameof(Importintegration))
                .WithDefaultValue(true)
                .ThenSubscribe(obs => obs.Select(b => new ChangeIntigrate(b))
                    .Mutate(workspace.Build).With(bm => bm.Intigrate, bm => ci => bm.SetIntigrate(ci.ToIntigrate))
                    .ObserveOnSelf()
                    .Subscribe(ii => Importintegration!.Set(ii.IsIntigrated)));

            #endregion

            #region Projects

            Projects = this.RegisterUiCollection<BuildProjectViewModel>(nameof(Projects))
                .BindToList(out var projectsSource);

            var projectPathSetter = new Subject<ProjectBuildpathRequest>().DisposeWith(this);

            projectPathSetter.Mutate(workspace.Build).With(bm => bm.ProjectPath,
                    bm => r => bm.SetProjectPath(r.Project, r.TargetPath))
                .ObserveOnSelf()
                .Subscribe(UpdatePath)
                .DisposeWith(this);


            BuildProjectViewModel GetBuildModel(Project p, ProjectFile file) => new(
                projectPathSetter ?? throw new InvalidOperationException("Flow was null"), file.FindProjectPath(p),
                p.ProjectName, localizer, dialogFactory, file.Source);

            void InitProjects(ProjectRest rest)
            {
                var file = rest.ProjectFile;

                projectsSource.Clear();
                projectsSource.AddRange(file.Projects.Select(p => GetBuildModel(p, file)));
            }

            void UpdatePath(ProjectPathChanged changed)
            {
                var model = Projects.FirstOrDefault(m => m.Project == changed.TargetProject);
                if (model == null) return;

                model.Path = changed.TargetPath;
            }

            this.RespondOnEventSource(workspace.Projects.NewProject,
                project => projectsSource.Add(GetBuildModel(project.Project, workspace.ProjectFile)));
            this.RespondOnEventSource(workspace.Projects.RemovedProject, project =>
            {
                var model = Projects.FirstOrDefault(m => m.Project == project.Project.ProjectName);
                if (model == null) return;

                projectsSource.Remove(model);
            });
            this.RespondOnEventSource(workspace.Source.SourceUpdate,
                updated => Projects.Foreach(m => m.UpdateSource(updated.Source)));

            #endregion

            #region Terminal

            TerminalMessages = this.RegisterUiCollection<string>(nameof(TerminalMessages))
                .BindToList(out var terminalMessages);
            var buildMessageLocalizer = new BuildMessageLocalizer(localizer);

            this.Receive<BuildMessage>(AddMessage);

            void AddMessage(BuildMessage message)
            {
                var locMsg = buildMessageLocalizer.Get(message);
                manager.Find(message.OperationId).NotNull().Subscribe(c => c.UpdateStatus(locMsg));
                terminalMessages.Add(locMsg);
            }

            #endregion

            #region Build

            var canBuild = true.ToRx();

            this.RespondOnEventSource(workspace.Source.SaveRequest, _ => InvokeCommand("StartBuild"));

            NewCommad
                .WithCanExecute(from value in canBuild
                    from reset in workspace.Source.ProjectReset
                    select value && !reset.ProjectFile.IsEmpty && !string.IsNullOrWhiteSpace(reset.ProjectFile.Source))
                .WithExecute(terminalMessages.Clear)
                .WithExecute(() => canBuild.Value = false)
                .WithFlow(obs => obs
                    .Select(_ => new BuildRequest(
                        manager.StartOperation(localizer.MainWindowodelBuildProjectOperation).Select(oc => oc.Id),
                        workspace.ProjectFile))
                    .ToActor(r => r.ProjectFile.Operator))
                .ThenRegister("StartBuild");

            this.Receive<BuildCompled>(BuildCompled);

            void BuildCompled(BuildCompled msg)
            {
                var (operationId, isFailed) = msg;
                manager.Find(operationId)
                    .NotNull()
                    .Subscribe(op =>
                    {
                        if (isFailed)
                            op.Failed();
                        else
                            op.Compled();

                        canBuild.Value = true;
                    });
            }

            #endregion

            #region Enable

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(false);
            this.RespondOnEventSource(workspace.Source.ProjectReset, r =>
            {
                IsEnabled += true;
                Importintegration += r.ProjectFile.BuildInfo.IntigrateProjects;
                InitProjects(r);
            });

            #endregion
        }

        public UIProperty<bool> IsEnabled { get; private set; }

        public UIProperty<bool> Importintegration { get; private set; }

        public UICollectionProperty<BuildProjectViewModel> Projects { get; }

        public UICollectionProperty<string> TerminalMessages { get; }

        public static string MakeRelativePath(string absolutePath, string pivotFolder)
        {
            //string folder = Path.IsPathRooted(pivotFolder)
            //    ? pivotFolder : Path.GetFullPath(pivotFolder);
            string folder = pivotFolder;
            Uri pathUri = new(absolutePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) folder += Path.DirectorySeparatorChar;
            Uri folderUri = new(folder);
            Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(
                relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private sealed class BuildMessageLocalizer
        {
            private readonly LocLocalizer _localizer;

            internal BuildMessageLocalizer(LocLocalizer localizer) => _localizer = localizer;

            internal string Get(BuildMessage msg)
            {
                return msg.Message switch
                {
                    BuildMessage.Ids.AgentCompled => msg.Agent + _localizer.MainWindowBuildProjectAgentCompled,
                    BuildMessage.Ids.GenerateCsFiles => msg.Agent + _localizer.MainWindowBuildProjectGenerateCsFile,
                    BuildMessage.Ids.GenerateLangFiles => msg.Agent + _localizer.MainWindowBuildProjectGenerateLangFile,
                    BuildMessage.Ids.NoData => _localizer.MainWindowBuildprojectNoData,
                    BuildMessage.Ids.GatherData => msg.Agent + _localizer.MainWindowBuildProjectGatherData,
                    _ => msg.Message
                };
            }
        }

        private sealed record ChangeIntigrate(bool ToIntigrate);
    }
}