using System.Reflection;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Host;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public static class HostExtensions
    {
        public static IApplicationBuilder AddStateManagment(this IApplicationBuilder builder, params Assembly[] assemblys)
        {
            return builder.ConfigureAutoFac(cb => cb.RegisterStateManager((b, context) =>
            {
                foreach (var assembly in assemblys) b.AddFromAssembly(assembly, context);
            }));
        }
    }
}