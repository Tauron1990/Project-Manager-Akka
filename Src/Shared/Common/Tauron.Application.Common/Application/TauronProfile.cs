using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Zio;

namespace Tauron.Application;


[PublicAPI]
public abstract class TauronProfile<TSelf> : ObservableObject, IEnumerable<string>
    where TSelf : TauronProfile<TSelf>
{
    private readonly ILogger<TSelf> _logger = TauronEnviroment.GetLogger<TSelf>();
    private readonly ImmutableDictionary<string, string> _settings;

    protected TauronProfile(Result<LoadResult> loadResult)
    {
        Application = loadResult.Select(d => d.Application);
        Name = loadResult.Select(d => d.Name);
        Dictionary = loadResult.Select(d => d.Directory);
        File = loadResult.Select(d => d.File);
        _settings = loadResult.Select(d => d.Settings).ValueOrDefault ?? ImmutableDictionary<string, string>.Empty;
    }

    public int Count => _settings.Count;

    public Result<string> Application { get; }

    public Result<string> Name { get; }

    protected Result<DirectoryEntry> Dictionary { get; }

    protected Result<FileEntry> File { get; }

    [Pure]
    public IEnumerator<string> GetEnumerator()
        => _settings.Select(pair => pair.Key).GetEnumerator();

    [Pure]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    protected abstract TSelf Create(in Result<LoadResult> newData);
    
    [Pure]
    public TSelf Delete()
    {
        _logger.LogInformation(
            "{Application} -- Delete Profile infos... {Path}",
            Application,
            Dictionary.Select(d => d.Path).Value.FullName.PathShorten(20));

        var newData = Application.Collect(
            Name,
            Dictionary,
            File,
            (app, name, dic, file) =>
            {
                dic.Delete();
                return new LoadResult(app, name, dic, file, ImmutableDictionary<string, string>.Empty);
            });
        
        if(newData is { HasError: true, Error: { } })
             _logger.LogError(newData.Error, "Error on Delete Profile");

        return Create(newData);
    }
    
    [Pure]
    protected static Result<LoadResult> Load(DirectoryEntry directoryEntry, string app, string name)
    {
        IlligalCharCheck(name);

        Name = name;
        Dictionary = _defaultPath.GetDirectory(Path.Combine(Application, name));
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

    private static Result<TSelf> IlligalCharCheck(string key, Func<Result<TSelf>> creator)
    {
        if(key.Contains('=', StringComparison.Ordinal))
            return Result.Error<TSelf>(new ArgumentException($"The Key ({key}) Contains an Illigal Char: =", nameof(key)));

        try
        {
            return creator();
        }
        catch (Exception e)
        {
            return Result.Error<TSelf>(e);
        }
    }

    public void Clear()
        => _settings.Clear();

    public readonly record struct LoadResult(string Application, string Name, DirectoryEntry Directory, FileEntry File, ImmutableDictionary<string, string> Settings);
}