using System;
using JetBrains.Annotations;

namespace Tauron.Application.Settings.Provider
{
    [PublicAPI]
    public static class ProviderConfig
    {
        private sealed class JsonProviderConfig : ISettingProviderConfiguration
        {
            private readonly string _fileName;

            internal JsonProviderConfig(string fileName, string scope)
            {
                _fileName = fileName;
                Scope = scope;
            }

            public string Scope { get; }
            public ISettingProvider Provider => new JsonProvider(_fileName);
        }

        public static Func<ISettingProviderConfiguration> Json(string scope, string fileName)
            => () => new JsonProviderConfig(fileName, scope);
    }
}