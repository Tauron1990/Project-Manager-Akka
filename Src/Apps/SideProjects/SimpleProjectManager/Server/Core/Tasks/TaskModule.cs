namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskModule : IModule
{
    public void Load(IServiceCollection collection)
        => collection.AddSingleton<TaskManagerCore>();
}