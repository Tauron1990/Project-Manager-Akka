
namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    public sealed record ToDeleteRevision(string Id, string BuckedId)
    {
        public ToDeleteRevision(string buckedId)
            : this(string.Empty, buckedId)
        {
        }
    }
}