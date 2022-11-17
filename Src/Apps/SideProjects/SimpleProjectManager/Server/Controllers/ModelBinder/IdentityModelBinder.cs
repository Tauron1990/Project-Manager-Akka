using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimpleProjectManager.Server.Controllers.ModelBinder;

public sealed class IdentityModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        try
        {
            string id = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue ?? string.Empty;
            bindingContext.Result = ModelBindingResult.Success(FastReflection.Shared.FastCreateInstance(bindingContext.ModelType, id));
        }
        catch (Exception)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            #pragma warning disable ERP022
        }
        #pragma warning restore ERP022

        return Task.CompletedTask;
    }
}