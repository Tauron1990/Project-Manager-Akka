using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI;
using Tauron.Application.Wpf.Implementation;

namespace Tauron.Application.Wpf;

[PublicAPI]
public sealed class WpfModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddTransient<IPackUriHelper, PackUriHelper>();
        collection.AddSingleton<IImageHelper, ImageHelper>();
        collection.AddScoped<IDialogFactory, DialogFactory>();
        collection.AddSingleton<CommonUIFramework, WpfFramework>();
        collection.AddSingleton(_ => WpfFramework.Dispatcher(System.Windows.Application.Current.Dispatcher));
    }
}