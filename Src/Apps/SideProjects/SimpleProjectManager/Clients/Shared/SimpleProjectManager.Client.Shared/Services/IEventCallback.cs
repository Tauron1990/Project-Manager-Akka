using System.Threading.Tasks;

namespace SimpleProjectManager.Client.Shared.Services;

public interface IEventCallback
{
    Task InvokeAsync();
}

public interface IEventCallback<in TParameter>
{
    Task InvokeAsync(TParameter parameter);
}