using Tauron.Host;

namespace Tauron.Application.Blazor
{
    public static class BlazorHostExtensions
    {
        public static IActorApplicationBuilder AddBlazorMVVM(this IActorApplicationBuilder builder) 
            => builder.AddModule<BlazorModule>();
    }
}