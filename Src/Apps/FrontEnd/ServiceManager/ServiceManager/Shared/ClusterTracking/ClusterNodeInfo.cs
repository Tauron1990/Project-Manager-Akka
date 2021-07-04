using Akka.Cluster;
using Newtonsoft.Json;
using Tauron.Application;

namespace ServiceManager.Shared.ClusterTracking
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

        [JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        public ClusterNodeInfo(string url, string status, string serviceType, string name)
        {
            Name = name;
            ServiceType = serviceType;
            Status = status;
            Url = url;
        }

        public ClusterNodeInfo(string url, MemberData memberData)
        {
            Url = url;
            Update(memberData);
        }

        private ClusterNodeInfo() => Url = string.Empty;

        private void Update(MemberData data)
        {
            var (member, name, serviceType) = data;

            if(!string.IsNullOrWhiteSpace(name))
                Name = name;
            ServiceType = serviceType.DisplayName;
            Status = member.Status.ToString();
        }

        public static implicit operator ClusterNodeInfo(MemberData data)
            => new(data.Member.Address.ToString(), data);
    }
}