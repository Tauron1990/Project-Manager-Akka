using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tauron.Application.Blazor;

public sealed class Scoped<TService> : IDisposable
    where TService : notnull
{
    public Scoped(IServiceProvider serviceProvider)
    {
        Provider = serviceProvider.CreateScope();
        Service = Provider.ServiceProvider.GetRequiredService<TService>();
    }

    public IServiceScope Provider { get; }

    public TService Service { get; }

    public void Dispose()
        => Provider.Dispose();

    public Scoped<TService> GetNewService<TNewService>(out TNewService newService)
        where TNewService : notnull
    {
        newService = Provider.ServiceProvider.GetRequiredService<TNewService>();

        return this;
    }

    public static implicit operator TService(Scoped<TService> scoped)
        => scoped.Service;
}