using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed partial record ProjectFile
    {
        public static ProjectFile NewProjectFile(IActorContext factory, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);

            return FromSource(source, actor);
        }

        public ProjectFile AddLanguage(Project project, ActiveLanguage language) => this with
                                                                                    {
                                                                                        Projects = Projects.Replace(
                                                                                            project,
                                                                                            project with
                                                                                            {
                                                                                                ActiveLanguages = project.ActiveLanguages.Add(language)
                                                                                            })
                                                                                    };

        public ProjectFile AddProject(Project project) => this with { Projects = Projects.Add(project) };

        public ProjectFile RemoveProject(Project project) => this with { Projects = Projects.Remove(project) };

        public ProjectFile AddImport(Project project, string toAdd) => this with
                                                                       {
                                                                           Projects = Projects.Replace(
                                                                               project,
                                                                               project with
                                                                               {
                                                                                   Imports = project.Imports.Add(toAdd)
                                                                               })
                                                                       };

        public ProjectFile ReplaceEntry(LocEntry? oldEntry, LocEntry? newEntry)
        {
            var projectName = oldEntry?.Project ?? newEntry?.Project;

            if (string.IsNullOrWhiteSpace(projectName)) return this;

            var entryName = oldEntry?.Key ?? newEntry?.Key;

            if (string.IsNullOrWhiteSpace(entryName)) return this;

            var old = Projects.Find(p => p.ProjectName == projectName);

            if (old == null)
                throw new KeyNotFoundException("Project Name not Found");

            if (oldEntry == null && newEntry != null)
                return this with { Projects = Projects.Replace(old, old with { Entries = old.Entries.Add(newEntry) }) };
            if (oldEntry != null && newEntry == null)
                return this with { Projects = Projects.Replace(old, old with { Entries = old.Entries.Remove(oldEntry) }) };
            if (oldEntry != null && newEntry != null)
                return this with
                       {
                           Projects = Projects.Replace(old, old with { Entries = old.Entries.Replace(oldEntry, newEntry) })
                       };

            return this;
        }

        // ReSharper disable once ReturnTypeCanBeNotNullable
        public string? FindProjectPath(Project project)
            => BuildInfo.ProjectPaths.FirstOrDefault(p => p.Key == project.ProjectName).Value;
    }
}