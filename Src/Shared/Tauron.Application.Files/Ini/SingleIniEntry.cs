using System;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Ini
{
    [PublicAPI]
    [Serializable]
    public sealed record SingleIniEntry(string Key, string? Value) : IniEntry(Key);
}