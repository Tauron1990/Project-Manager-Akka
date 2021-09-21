using System.Globalization;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Ini.Parser
{
    [PublicAPI]
    public class IniWriter
    {
        private readonly IniFile _file;
        private readonly TextWriter _writer;

        public IniWriter(IniFile file, TextWriter writer)
        {
            _file = file;
            _writer = writer;
        }

        public void Write()
        {
            try
            {
                foreach (var section in _file)
                {
                    _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}]", section.Name));
                    foreach (var iniEntry in section)
                        if (iniEntry is SingleIniEntry(var key, var data))
                        {
                            _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}={1}", key, data));
                        }
                        else
                        {
                            var (name, values) = (ListIniEntry)iniEntry;
                            foreach (var value in values)
                                _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}={1}", name, value));
                        }
                }
            }
            finally
            {
                _writer.Flush();
                _writer.Dispose();
            }
        }
    }
}