using System.Collections.Concurrent;
using Ionic.Zip;

namespace Tauron.Application.Files.Zip.Data;

public record ZipContext(ZipFile File, ZipEntry Entry, string Name, ZipDirectory? Parent);

public sealed record ZipDirectory(ZipFile File, ZipEntry? Entry, string Name, ZipDirectory? Parent, ConcurrentDictionary<string, ZipDirectory?> Dics, ConcurrentDictionary<string, ZipContext> Files) 
    : ZipContext(File, Entry, Name, Parent)
{
    public ZipDirectory(ZipFile file, ZipEntry? entry, string name, ZipDirectory? parent) 
        : this(file, entry, name, parent, new ConcurrentDictionary<string, ZipDirectory?>(), new ConcurrentDictionary<string, ZipContext>())
    {
    }
}