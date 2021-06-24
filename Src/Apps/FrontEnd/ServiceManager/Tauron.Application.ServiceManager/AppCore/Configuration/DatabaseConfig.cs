namespace Tauron.Application.ServiceManager.AppCore.Configuration
{
    public class DatabaseConfig : ObservableObject, IDatabaseConfig
    {
        private string _url;
        private bool _isReady;

        public string Url => _url;

        public bool IsReady => _isReady;

        public void SetUrl(string url)
        {

        }
    }
}