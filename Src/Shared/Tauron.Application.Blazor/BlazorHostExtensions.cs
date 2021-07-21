using JetBrains.Annotations;
using Tauron.AkkaHost;

namespace Tauron.Application.Blazor
{
    [PublicAPI]
    public static class BlazorHostExtensions
    {
        public static IActorApplicationBuilder AddBlazorMVVM(this IActorApplicationBuilder builder) 
            => builder.AddModule<BlazorModule>();
    }
}