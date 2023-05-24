namespace Tauron.Application.VirtualFiles;

public sealed class FeatureNotSupported : Error
{
    public FileSystemFeature Feature { get; }

    public FeatureNotSupported(FileSystemFeature feature)
    {
        Feature = feature;
        Message = $"Feature \"{feature}\" is not Supported by the Virtual File System";

        WithMetadata(nameof(Feature), feature);
    }
}