using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Dialogs;

#pragma warning disable GU0011

namespace Tauron.Application.CommonUI;

[PublicAPI]
public sealed class CommonUiModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddHostedService<UiAppService>();
        collection.TryAddSingleton<IDialogCoordinator, DialogCoordinator>();
    }
}