using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia
{
    [PublicAPI]
    public sealed class AvaloniaModule : IModule
    {
        public void Load(IServiceCollection collection)
        {
            collection.AddTransient(_ => AvaloniaFramework.UIDispatcher);
            collection.AddSingleton<CommonUIFramework, AvaloniaFramework>();
        }
    }
}