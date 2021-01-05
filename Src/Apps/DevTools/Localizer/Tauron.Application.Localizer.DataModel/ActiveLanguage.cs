using System.Globalization;
using System.IO;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed record ActiveLanguage(string Shortcut, string Name) : IWriteable
    {
        public static readonly ActiveLanguage Invariant = FromCulture(CultureInfo.InvariantCulture);
        

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shortcut);
            writer.Write(Name);
        }

        public static ActiveLanguage FromCulture(CultureInfo info) => new(info.Name, info.EnglishName);

        public CultureInfo ToCulture() => CultureInfo.GetCultureInfo(Shortcut);


        public static ActiveLanguage ReadFrom(BinaryReader reader)
        {
            var lang = new
            {
                Shortcut = reader.ReadString(),
                Name = reader.ReadString()
            };
            return new ActiveLanguage(lang.Shortcut, lang.Name);
        }
    }
}