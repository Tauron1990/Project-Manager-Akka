using System.IO.Compression;

namespace Tauron.Servicemnager.Networking;

public sealed record HostConfiguration(string Identifer, string TargetAdress, bool CreateShortcut)
{
    public const string DefaultFileName = "HostConfig.json";

    public static HostConfiguration Read()
    {
        using StreamReader stream = File.OpenText(DefaultFileName);

        return new HostConfiguration(
            stream.ReadLine() ?? string.Empty,
            stream.ReadLine() ?? string.Empty,
            bool.Parse(stream.ReadLine() ?? "false"));
    }

    public static void WriteInTo(string zipFile, string targetAdress, string identifer, bool createShortcut)
    {
        using ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);
        using var stream = new StreamWriter(zip.CreateEntry(DefaultFileName, CompressionLevel.Optimal).Open());

        stream.WriteLine(identifer);
        stream.WriteLine(targetAdress);
        stream.WriteLine(createShortcut);
        stream.Flush();
    }
}