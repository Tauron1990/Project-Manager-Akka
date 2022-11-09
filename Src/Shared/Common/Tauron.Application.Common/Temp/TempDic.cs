using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Stl;
using Tauron.Application;
using Tauron.Application.VirtualFiles;

namespace Tauron.Temp;

[PublicAPI]
public class TempDic : DisposeableBase, ITempDic
{
    public static readonly ITempDic Null = new TempDic();

    private readonly bool _deleteDic;
    private readonly IDirectory? _directory;

    private readonly Func<bool, string> _nameGenerator;
    private readonly ConcurrentDictionary<string, ITempDic> _tempDics = new();
    private readonly ConcurrentDictionary<string, ITempFile> _tempFiles = new();

    protected TempDic(IDirectory? directory, Option<ITempDic> parent, Func<bool, string> nameGenerator, bool deleteDic)
    {
        _nameGenerator = nameGenerator;
        _deleteDic = deleteDic;
        _directory = directory;
        Parent = parent;
    }

    private TempDic()
    {
        KeepAlive = true;
        _deleteDic = false;
        _nameGenerator = _ => string.Empty;
    }

    public PathInfo FullPath => _directory?.OriginalPath ?? string.Empty;
    public Option<ITempDic> Parent { get; }
    public bool KeepAlive { get; set; }

    public virtual ITempDic CreateDic(string name)
    {
        CheckNull();

        // ReSharper disable once HeapView.CanAvoidClosure
        return _tempDics.GetOrAdd(
            name,
            key =>
            {
                var dic = new TempDic(_directory?.GetDirectory(key), this, _nameGenerator, true);
                dic.TrackDispose(() => _tempDics.TryRemove(key, out _));

                return dic;
            });
    }

    public virtual ITempFile CreateFile(string name)
    {
        CheckNull();

        // ReSharper disable once HeapView.CanAvoidClosure
        return _tempFiles.GetOrAdd(
            name,
            key =>
            {
                var file = new TempFile(_directory?.GetFile(key), this);
                file.TrackDispose(() => _tempFiles.TryRemove(key, out _));

                return file;
            });
    }

    public ITempDic CreateDic() => CreateDic(_nameGenerator(false));

    public ITempFile CreateFile() => CreateFile(_nameGenerator(true));

    public void Clear()
    {
        if(string.IsNullOrWhiteSpace(FullPath))
            return;

        void TryDispose(IEnumerable<ITempInfo> toDispose)
        {
            foreach (ITempInfo entry in toDispose)
                try
                {
                    entry.Dispose();
                }
                catch (Exception exception)
                {
                    if(KeepAlive)
                        TauronEnviroment.GetLogger(GetType()).LogWarning(exception, "Error on Dispose Dic {Path}", entry.FullPath);
                    else
                        throw;
                }
        }

        try
        {
            var dics = _tempDics.Values;
            dics.Foreach(tempDic => tempDic.KeepAlive = KeepAlive);

            TryDispose(dics);
            TryDispose(_tempFiles.Values);
        }
        finally
        {
            _tempDics.Clear();
            _tempFiles.Clear();
        }
    }

    private void CheckNull()
    {
        if(string.IsNullOrEmpty(FullPath))
            throw new NotSupportedException("The Path is Empty");
    }

    protected override void DisposeCore(bool disposing)
    {
        if(!disposing) return;

        Clear();

        try
        {
            if(_deleteDic)
                _directory?.Clear();
        }
        catch (IOException) { }
    }
}