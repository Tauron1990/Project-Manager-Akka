using System;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.VirtualFiles.InMemory
{
    public sealed class InMemoryFileSystem : InMemoryDirectory, IVirtualFileSystem
    {
        public InMemoryFileSystem(string originalPath, string name, DataDirectory dic)
            : base(null, originalPath, name, dic) { }

        public void Dispose() { }

        public bool IsRealTime => true;

        public bool SaveAfterDispose
        {
            get => false;
            set => throw new NotSupportedException("In Memory does not Support Saving ");
        }

        public string Source => Name;

        public void Reload(string source)
            => throw new NotSupportedException("In Memory does not Reloading");
    }
}