using System.Collections.Generic;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application;

[PublicAPI]
public interface ITauronEnviroment
{
    IDirectory DefaultProfilePath { get; set; }

    IDirectory LocalApplicationData { get; }

    IDirectory LocalApplicationTempFolder { get; }

    IEnumerable<IDirectory> GetProfiles(string application);
}