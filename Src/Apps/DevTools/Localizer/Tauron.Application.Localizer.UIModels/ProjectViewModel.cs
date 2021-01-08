using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Autofac;
using DynamicData;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private string _project = string.Empty;

        public ProjectViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, LocLocalizer localizer, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            #region Init

            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).BindToList(out var languages);
            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys)).BindToList(out var projectEntrys);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).BindToList(out var  importprojects);

            var loadTrigger = new Subject<Unit>();

            Receive<IncommingEvent>(e => e.Action());

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(!workspace.ProjectFile.IsEmpty);
            SelectedIndex = RegisterProperty<int>(nameof(SelectedIndex));

            var self = Context.Self;

            void TryUpdateEntry((string ProjectName, string EntryName, ActiveLanguage Lang, string Content) data)
            {
                var (projectName, entryName, lang, content) = data;
                self.Tell(new UpdateRequest(entryName, lang, content, projectName));
            }


            void TryRemoveEntry((string ProjectName, string EntryName) data)
            {
                var (projectName, entryName) = data;
                self.Tell(new RemoveRequest(entryName, projectName));
            }

            OnPreRestart += (_, _) => Self.Tell(new InitProjectViewModel(workspace.Get(_project)));

            void InitProjectViewModel(InitProjectViewModel obj)
            {
                _project = obj.Project.ProjectName;

                languages.Edit(l =>
                               {
                                   l.Add(new ProjectViewLanguageModel(localizer.ProjectViewLanguageBoxFirstLabel, true));
                                   l.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
                               });
                SelectedIndex += 0;

                projectEntrys.AddRange(obj.Project.Entries.OrderBy(le => le.Key).Select(le => new ProjectEntryModel(obj.Project, le, TryUpdateEntry, TryRemoveEntry)));

                importprojects.AddRange(obj.Project.Imports);
                loadTrigger.OnNext(Unit.Default);
            }

            Receive<InitProjectViewModel>(InitProjectViewModel);

            #endregion

            #region New Entry

            IEnumerable<NewEntryInfoBase> GetEntrys()
            {
                var list = ImportetProjects!.ToList();
                list.Add(_project);

                var allEntrys = list.SelectMany(pro => workspace.Get(pro).Entries.Select(e => e.Key)).ToArray();

                return allEntrys.Select(e => new NewEntryInfo(e)).OfType<NewEntryInfoBase>()
                    .Concat(allEntrys
                        .Select(s => s.Split('_', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.Ordinal)
                        .Select(s => new NewEntrySuggestInfo(s!)));

            }

            void AddEntry(EntryAdd entry)
            {
                if(_project != entry.Entry.Project) return;

                projectEntrys.Add(new ProjectEntryModel(workspace.Get(_project), entry.Entry, TryUpdateEntry, TryRemoveEntry));
            }

            NewCommad.ThenFlow(ob => ob.Select(_ => GetEntrys())
                                       .Dialog(this).Of<INewEntryDialog, NewEntryDialogResult?>()
                                       .NotNull()
                                       .Mutate(workspace.Entrys).With(em => em.EntryAdd, em => res => em.NewEntry(_project, res.Name))
                                       .ObserveOnSelf()
                                       .Subscribe(AddEntry))
                     .ThenRegister("NewEntry");

            #endregion

            #region Remove Request

            void RemoveEntry(EntryRemove entry)
            {
                if (_project != entry.Entry.Project) return;

                var index = ProjectEntrys!.FindIndex(em => em.EntryName == entry.Entry.Key);
                if (index == -1) return;

                projectEntrys!.RemoveAt(index);
            }

            WhenReceive<RemoveRequest>(obs => obs.Mutate(workspace.Entrys).With(em => em.EntryRemove, em => rr => em.RemoveEntry(rr.ProjectName, rr.EntryName))
                                                 .ObserveOnSelf()
                                                 .Subscribe(RemoveEntry));
            #endregion

            #region Update Request

            void UpdateEntry(EntryUpdate obj)
            {
                if (_project != obj.Entry.Project) return;

                var model = ProjectEntrys!.FirstOrDefault(m => m.EntryName == obj.Entry.Key);
                model?.Update(obj.Entry);
            }

            WhenReceive<UpdateRequest>(obs => obs.Mutate(workspace.Entrys).With(em => em.EntryUpdate, em => ur => em.UpdateEntry(ur.ProjectName, ur.Language, ur.EntryName, ur.Content))
                                                 .ObserveOnSelf()
                                                 .Subscribe(UpdateEntry));

            #endregion

            #region Imports

            void AddImport(AddImport obj)
            {
                var (projectName, import) = obj;
                if (projectName != _project) return;
                importprojects!.Add(import);
            }

            IEnumerable<string> GetImportableProjects()
            {
                var pro = workspace.Get(_project);
                return workspace.ProjectFile.Projects.Select(p => p.ProjectName).Where(p => p != _project && !pro.Imports.Contains(p));
            }

            ImportSelectIndex = RegisterProperty<int>(nameof(ImportSelectIndex)).WithDefaultValue(-1);

            NewCommad.WithCanExecute(from trigger in loadTrigger
                                     select GetImportableProjects().Any())
                     .ThenFlow(ob => ob.Select(_ => GetImportableProjects())
                                       .Dialog(this).Of<IImportProjectDialog, ImportProjectDialogResult?>()
                                       .NotNull()
                                       .Mutate(workspace.Projects).With(pm => pm.NewImport, pm => r => pm.AddImport(_project, r.Project))
                                       .ObserveOnSelf()
                                       .Subscribe(AddImport))
                     .ThenRegister("AddImport");

            void RemoveImport(RemoveImport import)
            {
                var (targetProject, toRemove) = import;
                if (_project != targetProject) return;

                importprojects!.Remove(toRemove);
            }

            NewCommad.WithCanExecute(ImportSelectIndex.Select(i => i != -1))
                     .ThenFlow(ob => ob.Select(_ => new InitImportRemove(ImportetProjects[ImportSelectIndex]))
                                       .Mutate(workspace.Projects).With(pm => pm.RemoveImport, pm => ir => pm.TryRemoveImport(_project, ir.ToRemove))
                                       .Subscribe(RemoveImport))
               .ThenRegister("RemoveImport");

            workspace.Projects.NewImport.ToUnit().Subscribe(loadTrigger).DisposeWith(this);
            workspace.Projects.RemoveImport.ToUnit().Subscribe(loadTrigger).DisposeWith(this);

            #endregion

            #region AddLanguage

            void AddActiveLanguage(AddActiveLanguage language)
            {
                if (language.ProjectName != _project) return;

                languages.Add(new ProjectViewLanguageModel(language.ActiveLanguage.Name, false));

                foreach (var model in ProjectEntrys)
                    model.AddLanguage(language.ActiveLanguage);
            }


            NewCommad.ThenFlow(ob => ob.Select(_ => workspace.Get(_project).ActiveLanguages.Select(al => al.ToCulture()).ToArray())
                                       .Dialog(this).Of<ILanguageSelectorDialog, AddLanguageDialogResult?>()
                                       .NotNull()
                                       .Mutate(workspace.Projects).With(pm => pm.NewLanguage, pm => d => pm.AddLanguage(_project, d.CultureInfo))
                                       .Subscribe(AddActiveLanguage))
                     .ThenRegister("AddLanguage");

            #endregion
        }

        public UIProperty<bool> IsEnabled { get; }

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectIndex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        private sealed record InitImportRemove(string ToRemove);
        private sealed record UpdateRequest(string EntryName, ActiveLanguage Language, string Content, string ProjectName);
        private sealed record RemoveRequest(string EntryName, string ProjectName);
    }
}