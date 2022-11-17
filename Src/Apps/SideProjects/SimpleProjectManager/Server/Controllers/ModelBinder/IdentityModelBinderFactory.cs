using Akkatecture.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SimpleProjectManager.Server.Controllers.ModelBinder;

public sealed class IdentityModelBinderFactory : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Type model = context.Metadata.ModelType;

        if(model.IsAssignableTo(typeof(IIdentity)) && model.GetConstructor(new[] { typeof(string) }) != null)
            return new IdentityModelBinder();

        return null;
    }
}