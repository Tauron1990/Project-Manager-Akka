using System;
using System.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data
{
    public sealed class FileEntry : IDataElement
    {
        public MemoryStream? Data { get; private set; }
        
        public string? Name { get; private set; }

        public string ActualName
        {
            get
            {
                if(string.IsNullOrWhiteSpace(Name))
                    ThrowNotInitException();

                return Name!;
            }
        }

        public MemoryStream ActualData
        {
            get
            {
                if(Data is null)
                    ThrowNotInitException();

                return Data!;
            }
        }

        public FileEntry Init(string name, MemoryStream stream)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name dhould not be null", nameof(name));
            
            Name = name;
            Data = stream;

            return this;
        }
        
        public void Dispose()
        {
            Data?.Dispose();
            Data = null;
            Name = null;
        }

        private void ThrowNotInitException()
            => throw new InvalidOperationException("Entry not initialized");

        public void Recycle()
            => Dispose();
    }
}