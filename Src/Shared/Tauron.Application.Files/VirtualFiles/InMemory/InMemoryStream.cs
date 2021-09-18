using System.IO;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;

namespace Tauron.Application.Files.VirtualFiles.InMemory
{
    public sealed class InMemoryStream : MemoryStream
    {
        private readonly DataFile _data;

        public InMemoryStream(DataFile data)
            : base(data.Data?.Length ?? 0)
        {
            _data = data;

            if (data.Data == null) return;

            Write(data.Data);
            Seek(0, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing)
        {
            _data.Data = ToArray();

            base.Dispose(disposing);
        }
    }
}