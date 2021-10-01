using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.VirtualFiles.LocalFileSystem
{
    public class LocalFileSystem : LocalDirectory, IVirtualFileSystem
    {
        public override NodeType Type => NodeType.Root;

        public LocalFileSystem(string path)
            : base(path, () => null) { }

        public void Dispose() { }

        public bool IsRealTime => true;
        public bool SaveAfterDispose { get; set; }

        public string Source => OriginalPath;

        public void Reload(string source)
        {
            Reset(source, null);
        }
    }
}