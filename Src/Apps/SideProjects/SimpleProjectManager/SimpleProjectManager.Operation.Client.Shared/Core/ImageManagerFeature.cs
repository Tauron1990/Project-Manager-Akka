using System.Collections.Immutable;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Shared.Core;

public sealed class ImageManagerFeature : ActorFeatureBase<ImageManagerFeature.State>
{
    public sealed record State(
        string TargetPath, string CurrentPath, ImageManagerMode Mode, DataTransferManager Server, DataTransferManager Self,
        ImmutableDictionary<string, string> FileMapping);

    public static IPreparedFeature Create(string targetPath, ImageManagerMode mode)
        => Feature.Create(
            () => new ImageManagerFeature(),
            c => new State(targetPath, string.Empty, mode, DataTransferManager.Empty, DataTransferManager.New(c, "FileTransfer"), ImmutableDictionary<string, string>.Empty));

    protected override void ConfigImpl()
    {
        
    }
}