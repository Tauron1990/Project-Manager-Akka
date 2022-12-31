using System.Reflection;
using JetBrains.Annotations;
using Tauron.AkkaHost;

namespace Tauron.Application.Workshop.StateManagement.Akka;

[PublicAPI]
public static class HostExtensions
{
    public static IActorApplicationBuilder AddStateManagment(this IActorApplicationBuilder builder, params Assembly[] assemblys)
    {
        return builder.ConfigureServices(
            (_, cb) => cb.RegisterStateManager(
                (b, context) =>
                {
                    foreach (var assembly in assemblys)
                        b.AddFromAssembly(assembly, context);
                }));
    }
}