using System;
using System.Threading.Tasks;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewDatabseModel : ObservableObject
    {
        public string DatabaseUrl { get; set; }

        public string OriginalUrl { get; set; }

        public Func<string, string> ValidateUrl { get; };

        public void Reset();

        public Task Submit();

        public Task Init();
    }
}