using Hyperion.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Server.Controllers.FileUpload;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server;

[UsedImplicitly]
public sealed class MainModule : AkkaModule
{
    public override void Load(IActorApplicationBuilder builder)
    {
        builder.RegisterFeature<NameRegistry>(NameRegistryFeature.Factory(), "NameRegistry", () => SuperviserData.DefaultSuperviser);
        base.Load(builder);
    }

    public override void Load(IServiceCollection collection)
    {
        collection.TryAddScoped<FileUploadTransaction>();

    }
}