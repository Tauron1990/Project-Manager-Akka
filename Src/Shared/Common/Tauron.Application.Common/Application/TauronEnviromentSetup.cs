using System;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public static class TauronEnviromentSetup
{
    public static void Run(IServiceProvider provider)
    {
        TauronEnviroment.ServiceProvider = provider;
    }
}