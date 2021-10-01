using System;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.VirtualFiles.Core
{
    [PublicAPI]
    public abstract class FileSystemNodeBase<TInfo> : IFileSystemNode
        where TInfo : class
    {
        private Lazy<TInfo?> _infoObject;
        private Func<IDirectory?>? _parentDirectory;
        private IDirectory? _parentDirectoryInstance;

        protected FileSystemNodeBase(Func<IDirectory?> parentDirectory, string originalPath, string name)
        {
            _parentDirectory = parentDirectory;
            OriginalPath = originalPath;
            Name = name;
            _infoObject = new Lazy<TInfo?>(() => GetInfo(OriginalPath));
        }

        protected TInfo? InfoObject => _infoObject.Value;
        public abstract DateTime LastModified { get; }

        public IDirectory? ParentDirectory
        {
            get
            {
                if (_parentDirectoryInstance != null) return _parentDirectoryInstance;

                _parentDirectoryInstance = _parentDirectory?.Invoke();
                _parentDirectory = null;

                return _parentDirectoryInstance;
            }
        }

        public void Delete()
        {
            if (Exist)
                DeleteImpl();
        }

        public string Name { get; protected set; }

        public bool IsDirectory { get; private set; }

        public abstract NodeType Type { get; }
        public string OriginalPath { get; private set; }

        public abstract bool Exist { get; }

        protected abstract void DeleteImpl();

        protected abstract TInfo? GetInfo(string path);

        protected virtual void Reset(string path, IDirectory? parent)
        {
            _parentDirectory = () => parent;
            OriginalPath = path;
            _infoObject = new Lazy<TInfo?>(() => GetInfo(OriginalPath));
        }
    }
}