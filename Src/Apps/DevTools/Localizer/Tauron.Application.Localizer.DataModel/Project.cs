using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed record Project(ImmutableList<LocEntry> Entries, string ProjectName, ImmutableList<ActiveLanguage> ActiveLanguages, ImmutableList<string> Imports) : IWriteable
    {
        public Project()
            : this(ImmutableList<LocEntry>.Empty, string.Empty, ImmutableList<ActiveLanguage>.Empty, ImmutableList<string>.Empty) { }

        public Project(string name)
            : this()
            => ProjectName = name;

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);

            BinaryHelper.WriteList(ActiveLanguages, writer);
            BinaryHelper.WriteList(Entries, writer);
            BinaryHelper.WriteList(Imports, writer);
        }

        public ActiveLanguage GetActiveLanguage(string shortcut)
        {
            return ActiveLanguages.Find(al => al.Shortcut == shortcut) ?? ActiveLanguage.Invariant;
        }

        public static Project ReadFrom(BinaryReader reader)
        {
            var projectName = reader.ReadString();
            var activeLanguages = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom);
            var entries = BinaryHelper.Read(reader, LocEntry.ReadFrom);
            var imports = BinaryHelper.ReadString(reader);

            return new Project(entries, projectName, activeLanguages, imports);
        }
    }
}