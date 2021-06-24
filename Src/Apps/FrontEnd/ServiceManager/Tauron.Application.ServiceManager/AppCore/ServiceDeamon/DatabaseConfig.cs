using Tauron.Application.ServiceManager.AppCore.Settings;

namespace Tauron.Application.ServiceManager.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ObservableObject, IDatabaseConfig
    {
        private readonly ILocalConfiguration _configuration;
        private string _url;
        private bool _isReady;

        public string Url => _url;

        public bool IsReady => _isReady;

        public DatabaseConfig(ILocalConfiguration configuration)
        {
            _configuration = configuration;
            _url = _configuration.DatabaseUrl;
            _isReady = !string.IsNullOrWhiteSpace(_url);
        }

        public void SetUrl(string url)
        {

        }
    }
}