using System;

namespace Tauron.Application.Files.VirtualFiles.InMemory.Data
{
    public sealed class DataFile : DataElement
    {
        private byte[]? _data;

        public DataFile(string name) : base(name)
        {
        }

        public DateTime LastModifed { get; set; } = DateTime.Now;

        public byte[]? Data
        {
            get => _data;
            set
            {
                _data = value;
                LastModifed = DateTime.Now;
            }
        }
    }
}