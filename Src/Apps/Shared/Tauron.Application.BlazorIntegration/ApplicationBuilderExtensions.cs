using Microsoft.Extensions.Hosting;
using Tauron.Host;

namespace Tauron.Application.AspIntegration
{
    public static class ApplicationBuilderExtensions
    {
        public static IActorApplicationBuilder MapHostEnviroment(this IActorApplicationBuilder builder, IHostEnvironment environment)
            => builder.UpdateEnviroment(hr => new ActorHostEnviroment
                                              {
                                                  ApplicationName = string.IsNullOrWhiteSpace(hr.ApplicationName)
                                                      ? environment.ApplicationName
                                                      : hr.ApplicationName,
                                                  ContentRootPath = environment.ContentRootPath,
                                                  EnvironmentName = environment.EnvironmentName
                                              });
    }
}