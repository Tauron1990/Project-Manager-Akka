using System;
using System.Collections.Generic;

namespace Tauron.Application.Files.VirtualFiles.InMemory.Data
{
    public sealed class DataDirectory : DataElement
    {
        public DataDirectory(string name) : base(name)
        {
        }

        public List<DataFile> Files { get; set; } = new();

        public List<DataDirectory> Directories { get; set; } = new();

        public DateTime LastModifed { get; set; } = DateTime.MinValue;
    }
}