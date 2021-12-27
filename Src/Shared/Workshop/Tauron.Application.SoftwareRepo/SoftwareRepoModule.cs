using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Application.SoftwareRepo
{
    [PublicAPI]
    public sealed class SoftwareRepoModule : IModule
    {
        public void Load(IServiceCollection collection)
            => collection.AddTransient<IRepoFactory, RepoFactory>();
    }
}