
using System;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record ToDeleteRevision(string Id, string FilePath)
    {
        public ToDeleteRevision(string buckedId)
            : this(string.Empty, buckedId)
        {
        }

        public ToDeleteRevision()
            : this(string.Empty)
        {
            
        }
    }
}