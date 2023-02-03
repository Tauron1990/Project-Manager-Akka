using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public sealed class ProjectMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public ProjectMutator(MutatingEngine<MutatingContext<ProjectFile>> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;

            NewProject = engine.EventSource(
                mc => new AddProject(mc.GetChange<NewProjectChange>().Project),
                context => context.Change is NewProjectChange);
            RemovedProject = engine.EventSource(
                mc => new RemoveProject(mc.GetChange<RemoveProjectChange>().Project),
                context => context.Change is RemoveProjectChange);
            NewLanguage = engine.EventSource(
                mc => mc.GetChange<LanguageChange>().ToEventData(),
                context => context.Change is LanguageChange);
            NewImport = engine.EventSource(
                mc => mc.GetChange<AddImportChange>().ToEventData(),
                context => context.Change is AddImportChange);
            RemoveImport = engine.EventSource(
                mc => mc.GetChange<RemoveImportChange>().ToData(),
                context => context.Change is RemoveImportChange);

            NewLanguage.RespondOn(
                null,
                newLang =>
                {
                    var (activeLanguage, _) = newLang;

                    if (workspace.ProjectFile.GlobalLanguages.Contains(activeLanguage)) return;

                    if (!Projects.All(p => p.ActiveLanguages.Contains(activeLanguage))) return;

                    engine.Mutate(
                        nameof(AddLanguage) + "Global-Single",
                        obs => obs.Select(
                            context => context.Update(
                                new GlobalLanguageChange(activeLanguage),
                                context.Data with
                                {
                                    GlobalLanguages = context.Data.GlobalLanguages.Add(activeLanguage)
                                })));
                });
        }

        public IEnumerable<Project> Projects => _workspace.ProjectFile.Projects;

        public IEventSource<AddProject> NewProject { get; }

        public IEventSource<RemoveProject> RemovedProject { get; }

        public IEventSource<AddActiveLanguage> NewLanguage { get; }

        public IEventSource<AddImport> NewImport { get; }

        public IEventSource<RemoveImport> RemoveImport { get; }

        public void AddProject(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            _engine.Mutate(
                nameof(AddProject),
                obs => obs.Select(
                    context =>

                    {
                        var project = new Project(name)
                                      { ActiveLanguages = ImmutableList.CreateRange(context.Data.GlobalLanguages) };
                        var newFile = context.Data.AddProject(project);

                        return context.Update(new NewProjectChange(project), newFile);
                    }));
        }

        public void RemoveProject(string name)
        {
            _engine.Mutate(
                nameof(RemovedProject),
                obs => obs.Select(
                    context =>
                    {
                        Project project = context.Data.Projects
                            .First(p => string.Equals(p.ProjectName, name, System.StringComparison.Ordinal));
                        ProjectFile newFile = context.Data.RemoveProject(project);

                        return context.Update(new RemoveProjectChange(project), newFile);
                    }));
        }

        public void AddLanguage(CultureInfo? info)
        {
            if (info is null) return;

            ActiveLanguage newLang = ActiveLanguage.FromCulture(info);

            _engine.Mutate(
                nameof(AddLanguage) + "Global",
                obs => obs.Select(
                    context =>
                    {
                        context = context.Update(
                            new GlobalLanguageChange(newLang),
                            context.Data with { GlobalLanguages = context.Data.GlobalLanguages.Add(newLang) });

                        foreach (var project in context.Data.Projects.Select(p => p.ProjectName))
                            AddLanguage(project, info);

                        return context;
                    }));
        }

        public void AddLanguage(string proj, CultureInfo info)
        {
            _engine.Mutate(
                nameof(AddLanguage),
                obs => obs.Select(
                    context =>
                    {
                        Project project = context.Data.Projects
                            .First(p => string.Equals(p.ProjectName, proj, System.StringComparison.Ordinal));
                        ActiveLanguage lang = ActiveLanguage.FromCulture(info);

                        return project.ActiveLanguages.Contains(lang)
                            ? context
                            : context.Update(new LanguageChange(lang, proj), context.Data.AddLanguage(project, lang));
                    }));
        }

        public void AddImport(string projectName, string toAdd)
        {
            if (string.Equals(projectName, toAdd, System.StringComparison.Ordinal))
                return;

            _engine.Mutate(
                nameof(AddImport),
                obs => obs.Select(
                    context =>
                    {
                        var project = context.Data.Projects.First(p => string.Equals(p.ProjectName, projectName, System.StringComparison.Ordinal));

                        if (project.Imports.Contains(toAdd, System.StringComparer.Ordinal) || context.Data.Projects.All(p => !string.Equals(toAdd, p.ProjectName, System.StringComparison.Ordinal)))
                            return context;

                        return context.Update(
                            new AddImportChange(toAdd, projectName),
                            context.Data.AddImport(project, toAdd));
                    }));
        }

        public void TryRemoveImport(string projectName, string toRemove)
        {
            _engine.Mutate(
                nameof(RemoveImport),
                obs => obs.Select(
                    context =>
                    {
                        var pro = context.Data.Projects.Find(p => p.ProjectName == projectName);

                        if (pro == null) return context;
                        if (!pro.Imports.Contains(toRemove)) return context;

                        var newData = context.Data with
                                      {
                                          Projects = context.Data.Projects.Replace(
                                              pro,
                                              pro with { Imports = pro.Imports.Remove(toRemove) })
                                      };

                        return context.Update(new RemoveImportChange(projectName, toRemove), newData);
                    }));
        }
    }
}