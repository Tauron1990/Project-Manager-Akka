using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimpleProjectManager.Server.Controllers.ValueFactories;

public class IdValueProvider : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        return Task.CompletedTask;
    }
}