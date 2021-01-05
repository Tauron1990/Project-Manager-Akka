using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed record LocEntry(string Project, string Key, ImmutableDictionary<ActiveLanguage, string> Values) : IWriteable
    {
        public LocEntry(string project, string name)
            : this(project, name, ImmutableDictionary<ActiveLanguage, string>.Empty)
        { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Project);
            writer.Write(Key);

            BinaryHelper.WriteDic(Values, writer);
        }

        public static LocEntry ReadFrom(BinaryReader reader)
        {
            var builder = new 
            {
                Project = reader.ReadString(),
                Key = reader.ReadString(),
                Values = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom, binaryReader => binaryReader.ReadString())
            };


            return new LocEntry(builder.Project, builder.Key, builder.Values);
        }
    }
}