using System.IO;

namespace Tauron.Temp;

[PublicAPI]
public interface ITempFile : ITempInfo
{
    bool NoStreamDispose { get; set; }

    Stream Stream { get; }

    Stream NoDisposeStream { get; }
}