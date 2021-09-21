using System.Threading.Tasks;

namespace ServiceManager.Client.ViewModels.Models
{
    public interface ICookieService
    {
        void SetCookie<T>(string cookieName, T cookieValue);

        Task<T?> GetCookie<T>(string cookieName);

        void DeleteCookie(string cookieName);
    }
}