using Akka.Cluster;

namespace Tauron.Application.ServiceManager.PageData
{
    public sealed class ClusterNodeInfo : ObservableObject
    {
        private string _name = string.Empty;
        private string _status = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Url { get; }

        public ClusterNodeInfo(string url) => Url = url;
    }
}