using System.Collections.Generic;
using Zio;

namespace Tauron.Application;

[PublicAPI]
public interface ITauronEnviroment
{
    DirectoryEntry DefaultProfilePath { get; }

    DirectoryEntry LocalApplicationData { get; }

    DirectoryEntry LocalApplicationTempFolder { get; }

    IEnumerable<DirectoryEntry> GetProfiles(string application);
}