using Hyperion.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Server.Controllers.FileUpload;

namespace SimpleProjectManager.Server;

[UsedImplicitly]
public sealed class MainModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.TryAddScoped<FileUploadTransaction>();
        collection.RegisterFeature<NameRegistry>(NameRegistryFeature.Factory(),() => SuperviserData.DefaultSuperviser);
    }
}