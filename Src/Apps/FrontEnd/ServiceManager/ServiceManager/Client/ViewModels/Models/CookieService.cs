using System.Threading.Tasks;
using BlazorCarrot.Cookies;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace ServiceManager.Client.ViewModels.Models
{
    public sealed class CookieService : ICookieService
    {
        private readonly IJSRuntime _runtime;

        public CookieService(IJSRuntime runtime)
            => _runtime = runtime;

        public void SetCookie<T>(string cookieName, T cookieValue)
            => CookieController.SetCookie(_runtime, cookieName, JsonConvert.SerializeObject(cookieValue));

        public async Task<T?> GetCookie<T>(string cookieName)
            => JsonConvert.DeserializeObject<T>(await CookieController.GetCookie(_runtime, cookieName));

        public void DeleteCookie(string cookieName)
            => CookieController.DeleteCookie(_runtime, cookieName);
    }
}