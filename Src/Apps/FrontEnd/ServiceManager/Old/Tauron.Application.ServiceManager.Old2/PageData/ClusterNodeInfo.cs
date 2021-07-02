using Tauron.Application.ServiceManager.AppCore.ClusterTracking;

namespace Tauron.Application.ServiceManager.PageData
{
    public sealed class ClusterNodeInfo : ObservableObject
    {
        public static readonly ClusterNodeInfo Empty = new();

        private string _name = "Abrufen";
        private string _status = string.Empty;
        private string _serviceType = string.Empty;

        public string Name
        {
            get => _name;
            private set => SetProperty(ref _name, value);
        }

        public string ServiceType
        {
            get => _serviceType;
            private set => SetProperty(ref _serviceType, value);
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public string Url { get; }

        public ClusterNodeInfo(string url, MemberData memberData)
        {
            Url = url;
            Update(memberData);
        }

        private ClusterNodeInfo() => Url = string.Empty;

        public void Update(MemberData data)
        {
            var (member, name, serviceType) = data;

            if(!string.IsNullOrWhiteSpace(name))
                Name = name;
            ServiceType = serviceType.DisplayName;
            Status = member.Status.ToString();
        }
    }
}