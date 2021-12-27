using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterModelActor<TActor>(this IServiceCollection builder)
        where TActor : ActorModel
        => builder.RegisterDefaultActor<TActor>();
}