using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf;
using Tauron.ObservableExt;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, IOperationManager operationManager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            AppConfig config, IDialogFactory dialogFactory, IViewModel<CenterViewModel> model, IMainWindowCoordinator mainWindowCoordinator, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            Receive<IncommingEvent>(e => e.Action());

            var last = new RxVar<ProjectFile?>(null);
            var loadingOperation = new RxVar<OperationController?>(null);

            var self = Self;
            CenterView = this.RegisterViewModel(nameof(CenterView), model);
            workspace.Source.ProjectReset.RespondOn(null, pr => last.Value = pr.ProjectFile);

            #region Restarting

            OnPreRestart += (_, _) =>
            {
                if(last != null)
                    Self.Tell(last);

            };
            Receive<ProjectFile>(workspace.Reset);

            #endregion

            #region Operation Manager

            RunningOperations = RegisterProperty<IEnumerable<RunningOperation>>(nameof(RunningOperations)).WithDefaultValue(operationManager.RunningOperations);
            RenctFiles = RegisterProperty<RenctFilesCollection>(nameof(RenctFiles)).WithDefaultValue(new RenctFilesCollection(config, s => self.Tell(new InternlRenctFile(s))));

            NewCommad.WithExecute(operationManager.Clear, operationManager.ShouldClear()).ThenRegister("ClearOp");
            NewCommad.WithExecute(operationManager.CompledClear, operationManager.ShouldCompledClear()).ThenRegister("ClearAllOp");

            #endregion

            #region Save As

            IObservable<UpdateSource> SaveAsProject()
            {
                var targetFile = dialogFactory.ShowSaveFileDialog(null, true, false, true, "transp", true,
                    localizer.OpenFileDialogViewDialogFilter, true, true, localizer.MainWindowMainMenuFileSaveAs, Directory.GetCurrentDirectory());

                return targetFile.NotEmpty()
                                 .SelectMany(CheckSourceOk)
                                 .Where(d => d.Item1)
                                 .Select(r => new UpdateSource(r.Item2));
            }

            async Task<(bool, string)> CheckSourceOk(string source)
            {
                await UICall(() => dialogCoordinator.ShowMessage(localizer.CommonError, localizer.MainWindowModelLoadProjectSourceEmpty!));
                return (true, source);
            }

            NewCommad.WithCanExecute(last.Select(pf => pf != null && !pf.IsEmpty))
                     .ThenFlow(ob => ob.SelectMany(_ => SaveAsProject())
                                       .ToModel(CenterView))
                .ThenRegister("SaveAs");

            #endregion

            #region Open File

            IDisposable NewProjectFile(IObservable<SourceSelected> source)
                => source
                    .SelectMany(SourceSelectedFunc)
                         .NotNull()
                         .ObserveOnSelf()
                         .Select(ProjectLoaded)
                         .ToModel(CenterView!);


            Receive<InternlRenctFile>(o => OpentFileSource(o.File));

            IObservable<LoadedProjectFile?> SourceSelectedFunc(SourceSelected s)
            {
                if (s.Mode != OpenFileMode.OpenExistingFile) return NewFileSource(s.Source);
                OpentFileSource(s.Source);
                return Observable.Return<LoadedProjectFile?>(null);
            }

            void OpentFileSource(string? rawSource)
            {
                Observable.Return(rawSource)
                          .NotEmpty()
                          .SelectMany(CheckSourceOk)
                          .Select(p => p.Item2)
                          .Do(_ => mainWindowCoordinator.IsBusy = true)
                          .SelectMany(source => operationManager.StartOperation(string.Format(localizer.MainWindowModelLoadProjectOperation, Path.GetFileName(source)))
                                                                .Select(operationController => (operationController, source)))
                          .Do(_ =>
                              {
                                  if (!workspace.ProjectFile.IsEmpty)
                                      workspace.ProjectFile.Operator.Tell(ForceSave.Force(workspace.ProjectFile));
                              })
                          .ObserveOnSelf()
                          .Subscribe(pair => ProjectFile.BeginLoad(Context, pair.operationController.Id, pair.source, "Project_Operator"));
            }

            SupplyNewProjectFile? ProjectLoaded(LoadedProjectFile obj)
            {
                try
                {
                    if (loadingOperation!.Value != null)
                    {
                        if (obj.Ok)
                            loadingOperation.Value.Compled();
                        else
                        {
                            loadingOperation.Value.Failed(obj.ErrorReason?.Message ?? localizer.CommonError);
                            return null;
                        }
                    }

                    loadingOperation.Value = null;
                    if (obj.Ok) RenctFiles!.Value.AddNewFile(obj.ProjectFile.Source);

                    last!.Value = obj.ProjectFile;

                    return new SupplyNewProjectFile(obj.ProjectFile);
                }
                finally
                {
                    mainWindowCoordinator.IsBusy = false;
                }
            }

            NewCommad.WithCanExecute(loadingOperation.Select(oc => oc == null))
                     .ThenFlow(obs => NewProjectFile(obs.Dialog(this, TypedParameter.From(OpenFileMode.OpenExistingFile)).Of<IOpenFileDialog, string?>()
                                                        .Select(s => SourceSelected.From(s, OpenFileMode.OpenExistingFile))))
                     .ThenRegister("OpenFile");

            NewProjectFile(WhenReceive<SourceSelected>()).DisposeWith(this);

            #endregion

            #region New File

             IObservable<LoadedProjectFile?> NewFileSource(string? source)
             {
                 source ??= string.Empty;
                var data = new LoadedProjectFile(string.Empty, ProjectFile.NewProjectFile(Context, source, "Project_Operator"), null, true);

                 if (File.Exists(source))
                {
                    //TODO NewFile Filog Message
                    var result = UICall(async () => await dialogCoordinator.ShowMessage(localizer.CommonError!, "", null));

                    return result.Where(b => b == true).Do(_ => mainWindowCoordinator.IsBusy = true).Select(_ => data);
                }

                mainWindowCoordinator.IsBusy = true;
                return Observable.Return(data);
            }

            NewCommad.WithCanExecute(loadingOperation.Select(oc => oc == null))
                     .ThenFlow(obs => obs.Dialog(this, TypedParameter.From(OpenFileMode.OpenNewFile)).Of<IOpenFileDialog, string?>()
                                         .Select(s => SourceSelected.From(s, OpenFileMode.OpenNewFile))
                                         .ToSelf())
                     .ThenRegister("NewFile");

            #endregion

            #region Analyzing

            AnalyzerEntries = this.RegisterUiCollection<AnalyzerEntry>(nameof(AnalyzerEntries)).BindToList(out var analyterList);

            var builder = new AnalyzerEntryBuilder(localizer);

            void IssuesChanged(IssuesEvent obj)
            {
                analyterList.Edit(l =>
                                  {
                                      var (ruleName, issues) = obj;
                                      l.Remove(AnalyzerEntries.Where(e => e.RuleName == ruleName));
                                      l.AddRange(issues.Select(builder.Get));
                                  });
            }

            this.RespondOnEventSource(workspace.Analyzer.Issues, IssuesChanged);

            #endregion

            #region Build

            var buildModel = lifetimeScope.Resolve<IViewModel<BuildViewModel>>();
            buildModel.InitModel(Context, "Build-View");

            BuildModel = RegisterProperty<IViewModel<BuildViewModel>>(nameof(BuildModel)).WithDefaultValue(buildModel);

            #endregion
        }

        public UIProperty<IViewModel<BuildViewModel>> BuildModel { get; }

        private UICollectionProperty<AnalyzerEntry> AnalyzerEntries { get; }

        private UIProperty<IEnumerable<RunningOperation>> RunningOperations { get; }

        private UIProperty<RenctFilesCollection> RenctFiles { get; }

        private UIModel<CenterViewModel> CenterView { get; }


        private sealed class RenctFilesCollection : UIObservableCollection<RenctFile>
        {
            private readonly AppConfig _config;
            private readonly Action<string> _loader;

            public RenctFilesCollection(AppConfig config, Action<string> loader)
                : base(config.RenctFiles.Select(s => new RenctFile(s.Trim(), loader)))
            {
                _config = config;
                _loader = loader;
            }

            public void AddNewFile(string file)
            {
                file = file.Trim();

                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) return;

                if (this.Any(rf => rf.File == file)) return;

                if (Count > 10)
                    RemoveAt(Count - 1);

                Add(new RenctFile(file, _loader));
                _config.RenctFiles = ImmutableList<string>.Empty.AddRange(this.Select(rf => rf.File));
            }
        }

        private sealed class InternlRenctFile
        {
            public InternlRenctFile(string file) => File = file;

            public string File { get; }
        }

        private sealed class AnalyzerEntryBuilder
        {
            private readonly LocLocalizer _localizer;

            public AnalyzerEntryBuilder(LocLocalizer localizer) => _localizer = localizer;

            public AnalyzerEntry Get(Issue issue)
            {
                var builder = new AnalyzerEntry.Builder(issue.RuleName, issue.Project);

                return issue.IssueType switch
                {
                    Issues.EmptySource => builder.Entry(_localizer.MainWindowAnalyerRuleSourceName, _localizer.MainWindowAnalyerRuleSource),
                    _ => new AnalyzerEntry(_localizer.CommonUnkowen, issue.Project, issue.Data?.ToString() ?? string.Empty, _localizer.CommonUnkowen)
                };
            }
        }
    }
}