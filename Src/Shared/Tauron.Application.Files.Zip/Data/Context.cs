using System.Collections.Concurrent;
using Ionic.Zip;

namespace Tauron.Application.Files.Zip.Data;

public record ZipContext(ZipFile File, ZipEntry? Entry, string Name, ZipDirectoryContext? Parent);

public sealed record ZipDirectoryContext(ZipFile File, ZipEntry? Entry, string Name, ZipDirectoryContext? Parent, ConcurrentDictionary<string, ZipDirectoryContext?> Dics, ConcurrentDictionary<string, ZipContext> Files) 
    : ZipContext(File, Entry, Name, Parent)
{
    public ZipDirectoryContext(ZipFile file, ZipEntry? entry, string name, ZipDirectoryContext? parent) 
        : this(file, entry, name, parent, new ConcurrentDictionary<string, ZipDirectoryContext?>(), new ConcurrentDictionary<string, ZipContext>())
    {
    }
}