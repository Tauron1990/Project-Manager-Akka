﻿using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Actor;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class ProjectFile
    {
        public ImmutableList<Project> Projects { get; }

        public string Source { get; }

        public IActorRef Operator { get; }

        public bool IsEmpty => Operator.Equals(ActorRefs.Nobody);

        public ProjectFile()
        {
            Projects = ImmutableList<Project>.Empty;
            Source = string.Empty;
            Operator = ActorRefs.Nobody;
        }

        public ProjectFile(BinaryReader reader, string source, IActorRef op)
        {
            Source = source;
            Operator = op;

            var count = reader.ReadInt32();
            var builder = ImmutableList<Project>.Empty.ToBuilder();

            for (var i = 0; i < count; i++)
                builder.Add(new Project(reader));

            Projects = builder.ToImmutable();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Projects.Count);
            foreach (var project in Projects) 
                project.Write(writer);
        }

        public static void BeginLoad(IActorContext factory, string operationId, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            actor.Tell(new LoadProjectFile(operationId, source));
        }

        public static ProjectFile NewProjectFile(IActorContext factory, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            return new ProjectFile(ImmutableList<Project>.Empty, source, actor);
        }

        public ProjectFile AddLanguage(Project project, ActiveLanguage language)
        {
            var temp = project.WithActiveLanguages(project.ActiveLanguages.Add(language));
            return WithProjects(Projects.Replace(project, temp));
        }

        public ProjectFile AddProject(Project project) 
            => WithProjects(Projects.Add(project));

        public ProjectFile RemoveProject(Project project)
            => WithProjects(Projects.Remove(project));

        public ProjectFile AddImport(Project project, string toAdd) 
            => WithProjects(Projects.Replace(project, project.WithImports(project.Imports.Add(toAdd))));

        public ProjectFile ReplaceEntry(LocEntry? oldEntry, LocEntry? newEntry)
        {
            var projectName = oldEntry?.Project ?? newEntry?.Project;
            if (string.IsNullOrWhiteSpace(projectName)) return this;

            var entryName = oldEntry?.Key ?? newEntry?.Key;
            if (string.IsNullOrWhiteSpace(entryName)) return this;

            var old = Projects.Find(p => p.ProjectName == projectName);

            if (oldEntry == null && newEntry != null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Add(newEntry))));
            if (oldEntry != null && newEntry == null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Remove(oldEntry))));
            if (oldEntry != null && newEntry != null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Replace(oldEntry, newEntry))));

            return this;
        }
    }
}