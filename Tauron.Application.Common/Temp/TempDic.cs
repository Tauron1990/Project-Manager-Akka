﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Tauron.Temp
{
    public class TempDic : DisposeableBase, ITempDic
    {
        private readonly Func<string> _nameGenerator;
        private readonly bool _deleteDic;
        private readonly ConcurrentDictionary<string, ITempDic> _tempDics = new ConcurrentDictionary<string, ITempDic>();
        private readonly ConcurrentDictionary<string, ITempFile> _tempFiles = new ConcurrentDictionary<string, ITempFile>();

        public string FullPath { get; }
        public ITempDic? Parent { get; }
        public bool KeepAlive { get; set; }

        public TempDic(string fullPath, ITempDic? parent, Func<string> nameGenerator, bool deleteDic)
        {
            _nameGenerator = nameGenerator;
            _deleteDic = deleteDic;
            FullPath = fullPath;
            Parent = parent;

            fullPath.CreateDirectoryIfNotExis();
        }

        public ITempDic CreateDic(string name) 
            => _tempDics.GetOrAdd(name, s =>
            {
                var dic =  new TempDic(Path.Combine(FullPath, s), this, _nameGenerator, true);
                dic.TrackDispose(() => _tempDics.TryRemove(s, out _));
                return dic;
            });

        public ITempFile CreateFile(string name) 
            => _tempFiles.GetOrAdd(name, s =>
            {
                var file = new TempFile(Path.Combine(FullPath, s), this);
                file.TrackDispose(() => _tempFiles.TryRemove(s, out _));
                return file;
            });

        public ITempDic CreateDic() => CreateDic(_nameGenerator());

        public ITempFile CreateFile() => CreateFile(_nameGenerator());

        protected override void DisposeCore(bool disposing)
        {
            void TryDispose(IEnumerable<ITempInfo> toDispose)
            {
                foreach (var entry in toDispose)
                {
                    try
                    {
                        entry.Dispose();
                    }
                    catch (Exception e)
                    {
                        if (KeepAlive)
                            Log.ForContext(GetType()).Warning(e, $"Error on Dispose Dic {entry.FullPath}");
                        else
                            throw;
                    }
                }
            }

            if (!disposing) return;
            
            try
            {
                var dics = _tempDics.Values;
                dics.Foreach(t => t.KeepAlive = KeepAlive);

                TryDispose(dics);
                TryDispose(_tempFiles.Values);

                if(_deleteDic)
                    FullPath.DeleteDirectory(true);
            }
            finally
            {
                _tempDics.Clear();
                _tempFiles.Clear();
            }
        }
    }
}