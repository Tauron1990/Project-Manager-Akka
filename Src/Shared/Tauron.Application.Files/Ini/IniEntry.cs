using System;

namespace Tauron.Application.Files.Ini
{
    [Serializable]
    public abstract record IniEntry(string Key);
}