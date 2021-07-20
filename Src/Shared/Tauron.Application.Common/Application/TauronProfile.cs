using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Akka.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Host;

namespace Tauron.Application
{
    [PublicAPI]
    public abstract class TauronProfile : ObservableObject, IEnumerable<string>
    {
        private static readonly char[] ContentSplitter = {'='};

        private readonly string _defaultPath;
        private readonly ILogger<TauronProfile> _logger = ActorApplication.GetLogger<TauronProfile>();

        private readonly Dictionary<string, string> _settings = new();

        protected TauronProfile(string application, string defaultPath)
        {
            Application = Argument.NotNull(application, nameof(application));
            _defaultPath = Argument.NotNull(defaultPath, nameof(defaultPath));
        }

        public virtual string this[[NotNull] string key]
        {
            get => _settings[key];

            set
            {
                IlligalCharCheck(key);
                _settings[key] = value;
            }
        }

        public int Count => _settings.Count;

        public string Application { get; private set; }

        public Option<string> Name { get; private set; }

        protected Option<string> Dictionary { get; private set; }

        protected Option<string> FilePath { get; private set; }

        public IEnumerator<string> GetEnumerator()
        {
            return _settings.Select(k => k.Key).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Delete()
        {
            _settings.Clear();

            _logger.LogInformation($"{Application} -- Delete Profile infos... {Dictionary.GetOrElse(string.Empty).PathShorten(20)}");

            Dictionary.OnSuccess(s => s.DeleteDirectory());
        }

        public virtual void Load([NotNull] string name)
        {
            Argument.NotNull<object>(name, nameof(name));
            IlligalCharCheck(name);

            Name = name;
            Dictionary = _defaultPath.CombinePath(Application, name);
            Dictionary.OnSuccess(s => s.CreateDirectoryIfNotExis());
            FilePath = Dictionary.Select(s => s.CombinePath("Settings.db"));

            _logger.LogInformation($"{Application} -- Begin Load Profile infos... {FilePath.GetOrElse(string.Empty).PathShorten(20)}");

            _settings.Clear();
            foreach (var vals in FilePath.Value.EnumerateTextLinesIfExis()
                                         .Select(line => line.Split(ContentSplitter, 2))
                                         .Where(vals => vals.Length == 2))
            {
                _logger.LogInformation("key: {0} | Value {1}", vals[0], vals[1]);

                _settings[vals[0]] = vals[1];
            }
        }

        public virtual void Save()
        {
            _logger.LogInformation($"{Application} -- Begin Save Profile infos...");

            try
            {
                var writerOption = FilePath.Select(s => s.OpenTextWrite());

                if (!writerOption.HasValue) return;

                using var writer = writerOption.Value;

                foreach (var (key, value) in _settings)
                {
                    writer.WriteLine("{0}={1}", key, value);

                    _logger.LogInformation("key: {0} | Value {1}", key, value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on Profile Save");
            }
        }

        public virtual string? GetValue(string? defaultValue, [CallerMemberName] string? key = null)
        {
            var cKey = Argument.NotNull(key, nameof(key));

            IlligalCharCheck(cKey);

            return !_settings.ContainsKey(cKey) ? defaultValue : _settings[cKey];
        }

        public virtual int GetValue(int defaultValue, [CallerMemberName] string? key = null)
            => int.TryParse(GetValue(null, key), out var result) ? result : defaultValue;

        public virtual bool GetValue(bool defaultValue, [CallerMemberName] string? key = null)
            => bool.TryParse(GetValue(null, key), out var result) ? result : defaultValue;

        public virtual void SetVaue(object value, [CallerMemberName] string? key = null)
        {
            var cKey = Argument.NotNull(key, nameof(key));
            Argument.NotNull(value, nameof(value));
            IlligalCharCheck(cKey);

            _settings[cKey] = value.ToString() ?? string.Empty;
            OnPropertyChangedExplicit(cKey);
        }

        private static void IlligalCharCheck(string key)
        {
            Argument.Check(key.Contains('='),
                () => new ArgumentException($"The Key ({key}) Contains an Illigal Char: ="));
        }

        public void Clear()
        {
            _settings.Clear();
        }
    }
}