using System.Collections.Immutable;
using System.IO;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed record BuildInfo
        (bool IntigrateProjects, ImmutableDictionary<string, string> ProjectPaths) : IWriteable
    {
        public BuildInfo()
            : this(IntigrateProjects: true, ImmutableDictionary<string, string>.Empty)
        {
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(IntigrateProjects);
            BinaryHelper.WriteDic(ProjectPaths, writer);
        }

        public static BuildInfo ReadFrom(BinaryReader reader)
        {
            var builder = new
            {
                IntigrateProjects = reader.ReadBoolean(),
                ProjectPaths = BinaryHelper.Read(reader, r => r.ReadString(), r => r.ReadString())
            };

            return new BuildInfo(builder.IntigrateProjects, builder.ProjectPaths);
        }
    }
}