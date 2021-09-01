using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.UserManagment)]
    public interface ILogInManager
    {
        [Get(nameof(UpdateSession))]
        Task<string> UpdateSession([Query] string session);

        [Get(nameof(LogoutSession))]
        Task<string> LogoutSession();
    }
}