
using System;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record ToDeleteRevision(string Id, string FilePath)
    {
        public ToDeleteRevision()
            : this(string.Empty)
        {
            
        }

        public ToDeleteRevision(string filePath)
            : this(Guid.NewGuid().ToString("N"), filePath)
        {
            
        }
    }
}