using Autofac;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public static class ProjectionExtensions
{
    public static void RegisterProjection<TProjection>(this ContainerBuilder builder)
        where TProjection : IInitializeProjection
        => builder.RegisterType<TProjection>().As<TProjection, IInitializeProjection>().SingleInstance();
}