using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Stl;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application;

[PublicAPI]
public abstract class TauronProfile : ObservableObject, IEnumerable<string>
{
    private static readonly char[] ContentSplitter = { '=' };

    private readonly IDirectory _defaultPath;
    private readonly ILogger<TauronProfile> _logger = TauronEnviroment.GetLogger<TauronProfile>();

    private readonly Dictionary<string, string> _settings = new(StringComparer.Ordinal);

    protected TauronProfile(string application, IDirectory defaultPath)
    {
        Application = application;
        _defaultPath = defaultPath;
    }

    public virtual string this[string key]
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

    protected Option<IDirectory> Dictionary { get; private set; }

    protected Option<IFile> File { get; private set; }

    public IEnumerator<string> GetEnumerator()
        => _settings.Select(pair => pair.Key).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Delete()
    {
        _settings.Clear();

        _logger.LogInformation(
            "{Application} -- Delete Profile infos... {Path}",
            Application,
            Dictionary.Select(d => d.OriginalPath.Path).GetOrElse(string.Empty).PathShorten(20));

        Dictionary.OnSuccess(dic => dic.Delete());
    }

    public virtual void Load(string name)
    {
        IlligalCharCheck(name);

        Name = name;
        Dictionary = _defaultPath.GetDirectory(Path.Combine(Application, name)).OptionNotNull();
        File = Dictionary.Select(dic => dic.GetFile("Settings.db"));

        _logger.LogInformation(
            "{Application} -- Begin Load Profile infos... {Path}",
            Application,
            File.Select(f => f.OriginalPath.Path).GetOrElse(string.Empty).PathShorten(20));

        _settings.Clear();
        foreach (string[] vals in File.Value.EnumerateTextLinesIfExis()
                    .Select(line => line.Split(ContentSplitter, 2))
                    .Where(vals => vals.Length == 2))
        {
            _logger.LogInformation("key: {Key} | Value {Value}", vals[0], vals[1]);

            _settings[vals[0]] = vals[1];
        }
    }

    public virtual void Save()
    {
        _logger.LogInformation("{Application} -- Begin Save Profile infos...", Application);

        try
        {
            var writerOption = File.Select(path => new StreamWriter(path.Open(FileAccess.Write)));

            if(!writerOption.HasValue) return;

            using StreamWriter writer = writerOption.Value;

            foreach ((string key, string value) in _settings)
            {
                writer.WriteLine("{0}={1}", key, value);

                _logger.LogInformation("key: {Key} | Value {Value}", key, value);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error on Profile Save");
        }
    }

    public virtual string? GetValue(string? defaultValue, [CallerMemberName] string? key = null)
    {
        if(string.IsNullOrWhiteSpace(key)) return string.Empty;

        IlligalCharCheck(key);

        return !_settings.ContainsKey(key) ? defaultValue : _settings[key];
    }

    public virtual int GetValue(int defaultValue, IFormatProvider? provider = null, [CallerMemberName] string? key = null)
        => int.TryParse(GetValue(null, key), NumberStyles.Any, provider, out int result) ? result : defaultValue;

    #pragma warning disable AV1564
    public virtual bool GetValue(bool defaultValue, [CallerMemberName] string? key = null)
        #pragma warning restore AV1564
        => bool.TryParse(GetValue(null, key), out bool result) ? result : defaultValue;

    public virtual void SetVaue(object value, [CallerMemberName] string? key = null)
    {
        if(string.IsNullOrWhiteSpace(key)) return;

        IlligalCharCheck(key);

        _settings[key] = value.ToString() ?? string.Empty;
        OnPropertyChangedExplicit(key);
    }

    private static void IlligalCharCheck(string key)
    {
        if(key.Contains('=', StringComparison.Ordinal))
            throw new ArgumentException($"The Key ({key}) Contains an Illigal Char: =", nameof(key));
    }

    public void Clear()
        => _settings.Clear();
}