using System.Reflection;
using JetBrains.Annotations;
using Tauron.AkkaHost;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.Workshop;

[PublicAPI]
public static class HostExtensions
{
    public static IActorApplicationBuilder AddStateManagment(this IActorApplicationBuilder builder, params Assembly[] assemblys)
    {
        return builder.ConfigureAutoFac(
            cb => cb.RegisterStateManager(
                (b, context) =>
                {
                    foreach (var assembly in assemblys) b.AddFromAssembly(assembly, context);
                }));
    }
}