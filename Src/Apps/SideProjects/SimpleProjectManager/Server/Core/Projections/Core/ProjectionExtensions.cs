namespace SimpleProjectManager.Server.Core.Projections.Core;

public static class ProjectionExtensions
{
    public static void RegisterProjection<TProjection>(this IServiceCollection builder)
        where TProjection : class, IInitializeProjection
    {
        builder.AddSingleton<TProjection>();
        builder.AddSingleton<IInitializeProjection>(s => s.GetRequiredService<TProjection>());
    }
}