using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed partial record ProjectFile(ImmutableList<Project> Projects,
        ImmutableList<ActiveLanguage> GlobalLanguages, string Source, IActorRef Operator,
        BuildInfo BuildInfo) : IWriteable
    {
        public const int Version = 2;

        public ProjectFile()
            : this(ImmutableList<Project>.Empty, ImmutableList<ActiveLanguage>.Empty, string.Empty, ActorRefs.Nobody,
                new BuildInfo())
        {
        }

        private ProjectFile(string source, IActorRef op)
            : this()
        {
            Source = source;
            Operator = op;
        }

        public bool IsEmpty => Operator.Equals(ActorRefs.Nobody);

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            BinaryHelper.WriteList(Projects, writer);
            BinaryHelper.WriteList(GlobalLanguages, writer);
            BuildInfo.Write(writer);
        }

        public static ProjectFile FromSource(string source, IActorRef op) => new(source, op);

        public static ProjectFile ReadFile(BinaryReader reader, string source, IActorRef op)
        {
            var file = new ProjectFile(source, op);

            var vers = reader.ReadInt32();

            return file with
            {
                Projects = BinaryHelper.Read(reader, Project.ReadFrom),
                GlobalLanguages = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom),
                BuildInfo = vers == 1 ? new BuildInfo() : BuildInfo.ReadFrom(reader)
            };
        }

        public static void BeginLoad(IActorContext factory, string operationId, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            actor.Tell(new LoadProjectFile(operationId, source));
            Thread.Sleep(500);
        }
    }
}