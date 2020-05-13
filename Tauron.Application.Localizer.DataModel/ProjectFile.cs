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
    }
}