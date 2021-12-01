using Akkatecture.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimpleProjectManager.Server.Controllers.ModelBinder;

public sealed class IdentityModelBinderFactory : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var model = context.Metadata.ModelType;

        if (model.IsAssignableTo(typeof(IIdentity)) && model.GetConstructor(new[] { typeof(string) }) != null)
            return new IdentityModelBinder();

        return null;
    }
}

public sealed class IdentityModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        try
        {
            var id = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue ?? string.Empty;
            bindingContext.Result = ModelBindingResult.Success(FastReflection.Shared.FastCreateInstance(bindingContext.ModelType, id));
        }
        catch (Exception)
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}