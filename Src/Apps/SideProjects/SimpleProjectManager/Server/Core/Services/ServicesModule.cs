namespace SimpleProjectManager.Server.Core.Services;

public sealed class ServicesModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.AddSingleton<FileContentManager>();
        collection.AddScoped<CommitRegistrationTransaction>();
        collection.AddScoped<PreRegisterTransaction>();
    }
}